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
using map_app.Services;
using Mapsui;
using map_app.ViewModels;
using map_app.Services.Renders;

namespace map_app.Editing
{
    public class EditManager
    {
        public GraphicsLayer Layer { get; set; }
        public double CurrentOpacity { get; set; }
        public Color CurrentColor { get; set; }
        public MRect? Extent { get; set; }

        private readonly DragInfo _dragInfo = new();
        private readonly AddInfo _addInfo = new();
        private readonly RotateInfo _rotateInfo = new();
        private readonly ScaleInfo _scaleInfo = new();
        private readonly MainViewModel _mainViewModel;

        public EditManager(MainViewModel main)
        {
            _mainViewModel = main;
            Layer = main.Graphics;            
            CurrentColor = Color.Black;
            CurrentOpacity = 1;
        }

        public EditMode EditMode { get; set; }

        public int VertexRadius { get; set; } = 12;

        public bool EndEdit()
        {
            if (_addInfo.Feature is null) return false;
            if (_addInfo.Feature.Extent is null) return false;
            if (_addInfo.Vertices is null) return false;
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
            _addInfo.Feature = null;
            _addInfo.Vertex = null;
            EditMode = nextMode;
            return false;
        }

        internal void HoveringVertex(MapInfo? mapInfo)
        {
            if (_addInfo.Vertex != null)
            {
                _addInfo.Vertex.SetXY(mapInfo?.WorldPosition);
                _addInfo.Feature?.RenderedGeometry.Clear();
                _addInfo.Feature?.RerenderGeometry();
                Layer.DataHasChanged();
            }
        }

        public bool AddVertex(Coordinate worldPosition)
        {
            if (!Extent?.Contains(worldPosition.ToMPoint()) ?? false)
                return false;

            if (EditMode == EditMode.AddPoint)
            {
                Layer.Add(new PointGraphic(new[] { worldPosition }.ToList()) { StyleColor = CurrentColor, Opacity = CurrentOpacity });
                Layer.LayersFeatureHasChanged();
            }
            else if (EditMode == EditMode.AddPolygon)
                AddGraphic(worldPosition, typeof(PolygonGraphic), EditMode.DrawingPolygon);
            else if (EditMode == EditMode.DrawingPolygon || EditMode == EditMode.DrawingOrthodromeLine)
                AddNewStepPoint(worldPosition, _addInfo.Feature as IHoveringGraphic);
            else if (EditMode == EditMode.AddOrthodromeLine)
                AddGraphic(worldPosition, typeof(OrthodromeGraphic), EditMode.DrawingOrthodromeLine);
            else if (EditMode == EditMode.AddRectangle)
                AddGraphic(worldPosition, typeof(RectangleGraphic), EditMode.DrawingRectangle);
            else if (EditMode == EditMode.DrawingRectangle)
            {
                AddNewStepPoint(worldPosition, _addInfo.Feature as IHoveringGraphic);
                EndEdit();
            }

            return false;
        }

        private void AddGraphic(Coordinate worldPosition, Type graphicType, EditMode drawingMode)
        {
            _addInfo.Vertices = new List<Coordinate> { worldPosition };
            var graphic = CreateGraphic(graphicType);
            _addInfo.Feature = graphic;
            _addInfo.Vertex = worldPosition.Copy();
            if (graphic is not IHoveringGraphic hoveringGraphic)
                throw new ArgumentException($"Type of graphic {graphicType} must implement IHoveringGraphic");
            hoveringGraphic.HoverVertex = _addInfo.Vertex;
            Layer.Add(_addInfo.Feature);
            Layer.LayersFeatureHasChanged();
            EditMode = drawingMode;
        }

        private BaseGraphic CreateGraphic(Type graphicType)
        {
            var graphic = (BaseGraphic?)Activator.CreateInstance(graphicType, _addInfo.Vertices!.Single())
                ?? throw new Exception($"Activator can not create instance of type \"{graphicType}\"");
            graphic.StyleColor = CurrentColor;
            graphic.Opacity = CurrentOpacity;
            var distanceLabels = graphic.Styles.FirstOrDefault(x => x is LabelDistanceStyle);
            if (distanceLabels is not null)
                distanceLabels.Enabled = _mainViewModel.IsRulerActivated;
            return graphic;
        }

        private void AddNewStepPoint(Coordinate worldPosition, IHoveringGraphic? target)
        {
            if (target is null) return;
            if (_addInfo.Vertices is null) return;

            // Set the final position of the 'hover' vertex (that was already part of the geometry)
            _addInfo.Vertex.SetXY(worldPosition);
            target.HoverVertex = worldPosition.Copy();
            _addInfo.Vertex = target.HoverVertex;
            _addInfo.Vertices.Add(_addInfo.Vertex);
            target.AddPoint(worldPosition);

            _addInfo.Feature?.RenderedGeometry.Clear();
            Layer.DataHasChanged();
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
            if (EditMode != EditMode.Modify) return false;
            if (mapInfo.Feature == null) return false;
            if (mapInfo.Feature is not BaseGraphic geometryFeature || mapInfo.Feature is OrthodromeGraphic) return false;
            if (mapInfo.WorldPosition == null) return true;

            _dragInfo.Feature = geometryFeature;
            _dragInfo.StartOffsetsToVertexes = new MPoint[_dragInfo.Feature.Coordinates.Count];
            
            for (var i = 0; i < _dragInfo.Feature.Coordinates.Count; i++)
                _dragInfo.StartOffsetsToVertexes[i] = mapInfo.WorldPosition - _dragInfo.Feature.Coordinates[i].ToMPoint();

            return true;
        }

        public bool DraggingEntirely(Point? worldPosition)
        {
            if (EditMode != EditMode.Modify || 
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
            if (EditMode == EditMode.Modify && _dragInfo.Feature != null)
            {
                _dragInfo.Feature.Geometry?.GeometryChanged();
                _dragInfo.Feature = null;
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
                        Layer.DataHasChanged();
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
                    Layer.DataHasChanged();
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
            Layer.DataHasChanged();

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
            Layer.DataHasChanged();

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
            if (EditMode == EditMode.DrawingOrthodromeLine ||
            EditMode == EditMode.DrawingPolygon ||
            EditMode == EditMode.DrawingRectangle)
            {
                if (_addInfo.Feature is null) return;
                Layer.TryRemove(_addInfo.Feature);
                EndEdit();
            }
        }
    }
}