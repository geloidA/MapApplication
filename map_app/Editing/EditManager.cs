using System;
using System.Collections.Generic;
using System.Linq;
using map_app.Editing.Extensions;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.UI;
using map_app.Models;
using NetTopologySuite.Geometries;
using Mapsui.Styles;
using Mapsui;
using map_app.ViewModels;
using map_app.Services.Renders;
using map_app.Services.Layers;

namespace map_app.Editing;

public class EditManager
{
    private readonly DragInfo _dragInfo = new();
    private readonly AddInfo _addInfo = new();
    private readonly RotateInfo _rotateInfo = new();
    private readonly ScaleInfo _scaleInfo = new();
    private readonly MainViewModel _mainVM;

    public GraphicsLayer GraphicLayer { get; }

    public Color Color { get; set; } = Color.Black;

    public MRect Extent { get; }

    public EditManager(MainViewModel main, MRect extent)
    {
        _mainVM = main;
        GraphicLayer = main.GraphicsLayer;
        Extent = extent;
    }

    public EditMode EditMode { get; set; }

    public int VertexRadius { get; set; } = 4;

    public void EndIncompleteEditing()
    {
        if (_addInfo.CurrentVertex is null) return;
        AddVertex(_addInfo.CurrentVertex);
        EndEdit();
    }

    public bool EndEdit()
    {
        if (_addInfo.Feature is null) return false;
        if (_addInfo.Feature.Extent is null) return false;
        if (_addInfo.Feature is IHoveringGraphic feature)
            feature.HoverVertex = null;

        return EditMode switch
        {
            EditMode.DrawingOrthodromeLine => EndEdit(EditMode.AddOrthodromeLine),
            EditMode.DrawingPolygon => EndEdit(EditMode.AddPolygon),
            EditMode.DrawingRectangle => EndEdit(EditMode.AddRectangle),
            _ => false
        };
    }

    private bool EndEdit(EditMode nextMode)
    {
        _addInfo.Feature!.RerenderGeometry();
        _addInfo.Feature = null;
        _addInfo.CurrentVertex = null;
        EditMode = nextMode;
        return false;
    }

    internal void HoveringVertex(MapInfo? mapInfo)
    {
        if (_addInfo.Feature is not IHoveringGraphic || _addInfo.CurrentVertex == null) return;
        _addInfo.CurrentVertex.SetXY(mapInfo?.WorldPosition);
        _addInfo.Feature?.RenderedGeometry.Clear();
        _addInfo.Feature?.RerenderGeometry();
        GraphicLayer.DataHasChanged();
    }

    public bool AddVertex(Coordinate worldPosition)
    {
        if (!Extent?.Contains(worldPosition.ToMPoint()) ?? false)
            return false;

        return EditMode switch
        {
            EditMode.AddPoint => AddGraphic(worldPosition, typeof(PointGraphic), EditMode.AddPoint),
            EditMode.AddPolygon => AddGraphic(worldPosition, typeof(PolygonGraphic), EditMode.DrawingPolygon),
            EditMode.AddOrthodromeLine => AddGraphic(worldPosition, typeof(OrthodromeGraphic), EditMode.DrawingOrthodromeLine),
            EditMode.AddRectangle => AddGraphic(worldPosition, typeof(RectangleGraphic), EditMode.DrawingRectangle),
            EditMode.DrawingPolygon => AddNewStepPoint(worldPosition, _addInfo.Feature as IHoveringGraphic),
            EditMode.DrawingOrthodromeLine => AddNewStepPoint(worldPosition, _addInfo.Feature as IHoveringGraphic),
            EditMode.DrawingRectangle => AddNewStepPointAndEndEdit(worldPosition, _addInfo.Feature as IHoveringGraphic),
            _ => false
        };
    }

    private bool AddGraphic(Coordinate worldPosition, Type graphicType, EditMode drawingMode)
    {
        var graphic = CreateGraphic(graphicType, worldPosition);
        _addInfo.Feature = graphic;
        _addInfo.CurrentVertex = worldPosition.Copy();
        if (graphic is IHoveringGraphic hoveringGraphic)
            hoveringGraphic.HoverVertex = _addInfo.CurrentVertex;
        GraphicLayer.Add(_addInfo.Feature);
        EditMode = drawingMode;
        return false;
    }

    private BaseGraphic CreateGraphic(Type graphicType, Coordinate startPosition)
    {
        var graphic = (BaseGraphic?)Activator.CreateInstance(graphicType, startPosition)
            ?? throw new Exception($"Activator can not create instance of type \"{graphicType}\"");
        graphic.StyleColor = Color;
        var distanceLabels = graphic.Styles.FirstOrDefault(x => x is LabelDistanceStyle);
        if (distanceLabels is not null)
            distanceLabels.Enabled = _mainVM.IsRulerActivated;
        return graphic;
    }

    private bool AddNewStepPoint(Coordinate worldPosition, IHoveringGraphic? target)
    {
        if (target is null) return false;
        // Set the final position of the 'hover' vertex (that was already part of the geometry)
        _addInfo.CurrentVertex.SetXY(worldPosition);
        target.HoverVertex = worldPosition.Copy();
        _addInfo.CurrentVertex = target.HoverVertex;
        target.AddPoint(worldPosition);
        _addInfo.Feature?.RenderedGeometry.Clear();
        return false;
    }

    private bool AddNewStepPointAndEndEdit(Coordinate worldPosition, IHoveringGraphic? target)
    {
        AddNewStepPoint(worldPosition, target);
        return EndEdit();
    }

    private static Coordinate? FindVertexTouched(MapInfo mapInfo, IEnumerable<Coordinate> vertices, double screenDistance)
    {
        if (mapInfo.WorldPosition == null)
            return null;

        return vertices.OrderBy(v => v.Distance(mapInfo.WorldPosition.ToCoordinate()))
            .FirstOrDefault(v => v.Distance(mapInfo.WorldPosition.ToCoordinate()) < mapInfo.Resolution * screenDistance);
    }

    public bool StartDraggingEntirely(MapInfo mapInfo, double screenDistance)
    {
        if (EditMode != EditMode.Drag) return false;
        if (mapInfo.Feature == null) return false;
        if (mapInfo.Feature is not BaseGraphic geometryFeature || mapInfo.Feature is OrthodromeGraphic) return false;
        if (mapInfo.WorldPosition == null) return true;
        _mainVM.DataState = DataState.Unsaved;

        _dragInfo.Feature = geometryFeature;
        _dragInfo.StartOffsetsToVertexes = new MPoint[_dragInfo.Feature.Coordinates.Count];

        for (var i = 0; i < _dragInfo.Feature.Coordinates.Count; i++)
            _dragInfo.StartOffsetsToVertexes[i] = mapInfo.WorldPosition - _dragInfo.Feature.Coordinates[i].ToMPoint();

        return true;
    }

    public bool DraggingEntirely(Point? worldPosition)
    {
        if (EditMode != EditMode.Drag ||
            _dragInfo.Feature == null ||
            worldPosition == null ||
            _dragInfo.StartOffsetsToVertexes == null)
            return false;

        var i = 0;
        foreach (var coordinate in _dragInfo.Feature.Coordinates)
        {
            coordinate.SetXY(worldPosition.ToMPoint() - _dragInfo.StartOffsetsToVertexes[i]);
            i++;
        }
        _dragInfo.Feature.RerenderGeometry();
        return true;
    }

    public void StopDragging()
    {
        if (EditMode == EditMode.Drag && _dragInfo.Feature != null)
        {
            _dragInfo.Feature.Geometry?.GeometryChanged();
            _dragInfo.Feature = null;
            _dragInfo.StartOffsetsToVertexes = null;
        }
    }

    public bool TryDeleteCoordinate(MapInfo? mapInfo, double screenDistance)
    {
        if (mapInfo?.Feature is GeometryFeature geometryFeature)
        {
            var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainCoordinates() ?? new List<Coordinate>(), screenDistance);
            if (vertexTouched != null)
            {
                var vertices = geometryFeature.Geometry?.MainCoordinates() ?? new List<Coordinate>();
                var index = vertices.IndexOf(vertexTouched);
                if (index >= 0)
                {
                    geometryFeature.Geometry = geometryFeature.Geometry.DeleteCoordinate(index);
                    geometryFeature.RenderedGeometry.Clear();
                    GraphicLayer.DataHasChanged();
                }
            }
        }

        return false;
    }

    public bool TryInsertCoordinate(MapInfo? mapInfo)
    {
        if (mapInfo?.WorldPosition is null) return false;

        if (mapInfo.Feature is GeometryFeature geometryFeature)
        {
            if (geometryFeature.Geometry is null) return false;

            var vertices = geometryFeature.Geometry.MainCoordinates();
            if (EditHelper.ShouldInsert(mapInfo.WorldPosition, mapInfo.Resolution, vertices, VertexRadius, out var segment))
            {
                geometryFeature.Geometry = geometryFeature.Geometry.InsertCoordinate(mapInfo.WorldPosition.ToCoordinate3D()
                    ?? throw new NullReferenceException("Inserted mapInfo was null"), segment);
                geometryFeature.RenderedGeometry.Clear();
                GraphicLayer.DataHasChanged();
            }
        }
        return false;
    }

    public bool StartRotating(MapInfo mapInfo)
    {
        if (mapInfo.Feature is BaseGraphic geometryFeature)
        {
            if (EditMode != EditMode.Rotate) return false;

            _rotateInfo.Feature = geometryFeature;
            _rotateInfo.PreviousPosition = mapInfo.WorldPosition.ToPoint();
            _rotateInfo.Center = geometryFeature.Geometry?.Centroid;
        }
        return true; // to signal pan lock
    }

    public bool Rotating(Point? worldPosition)
    {
        if (EditMode != EditMode.Rotate || _rotateInfo.Feature == null || worldPosition == null || _rotateInfo.Center == null || _rotateInfo.PreviousPosition == null) return false;

        var previousVector = new Point(
            _rotateInfo.Center.X - _rotateInfo.PreviousPosition.X,
            _rotateInfo.Center.Y - _rotateInfo.PreviousPosition.Y);
        var currentVector = new Point(
            _rotateInfo.Center.X - worldPosition.X,
            _rotateInfo.Center.Y - worldPosition.Y);
        var degrees = AngleBetween(currentVector, previousVector);

        if (_rotateInfo.Feature.Geometry != null)
            Geomorpher.Rotate(_rotateInfo.Feature.Geometry, degrees, _rotateInfo.Center);

        _rotateInfo.PreviousPosition = worldPosition;

        _rotateInfo.Feature.RenderedGeometry.Clear();
        GraphicLayer.DataHasChanged();

        return true; // to signal pan lock
    }

    public void StopRotating()
    {
        if (EditMode == EditMode.Rotate && _rotateInfo.Feature != null)
        {
            _rotateInfo.Feature.Geometry?.GeometryChanged();
            _rotateInfo.Feature = null;
        }
    }

    public static double AngleBetween(Point vector1, Point vector2)
    {
        var sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
        var cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

        return Math.Atan2(sin, cos) * (180 / Math.PI);
    }

    public bool StartScaling(MapInfo mapInfo)
    {
        if (mapInfo.Feature is BaseGraphic geometryFeature)
        {
            if (EditMode != EditMode.Scale) return false;

            _scaleInfo.Feature = geometryFeature;
            _scaleInfo.PreviousPosition = mapInfo.WorldPosition.ToPoint();
            _scaleInfo.Center = geometryFeature.Geometry?.Centroid;
        }

        return true; // to signal pan lock
    }

    public bool Scaling(Point? worldPosition)
    {
        if (EditMode != EditMode.Scale || _scaleInfo.Feature == null || worldPosition == null || _scaleInfo.PreviousPosition == null || _scaleInfo.Center == null) return false;

        var scale =
            _scaleInfo.Center.Distance(worldPosition) /
            _scaleInfo.Center.Distance(_scaleInfo.PreviousPosition);


        if (_scaleInfo.Feature.Geometry != null)
            Geomorpher.Scale(_scaleInfo.Feature.Geometry, scale, _scaleInfo.Center);

        _scaleInfo.PreviousPosition = worldPosition;

        _scaleInfo.Feature.RenderedGeometry.Clear();
        GraphicLayer.DataHasChanged();

        return true; // to signal pan lock
    }

    public void StopScaling()
    {
        if (EditMode == EditMode.Scale && _scaleInfo.Feature != null)
        {
            _scaleInfo.Feature.Geometry?.GeometryChanged();
            _scaleInfo.Feature = null;
        }
    }

    internal void CancelDrawing()
    {
        if (EditMode == EditMode.None || !EditMode.DrawingMode.HasFlag(EditMode)) return;
        if (_addInfo.Feature is null) return;
        GraphicLayer.TryRemove(_addInfo.Feature);
        EditMode = EditMode.GetAddMode();
    }
}