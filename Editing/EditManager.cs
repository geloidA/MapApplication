using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui;
using map_app.Editing.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.UI;
using map_app.Models;
using Mapsui.Utilities;
using NetTopologySuite.Geometries;

namespace map_app.Editing
{
    public enum EditMode
    {
        None,
        AddPoint,
        AddLine,
        DrawingLine,
        AddPolygon,
        DrawingPolygon,
        Modify,
        Rotate,
        Scale,
        AddOrthodromeLine,
        DrawingOrthodromeLine
    }

    public class EditManager
    {
        public WritableLayer? Layer { get; set; }

        private readonly DragInfo _dragInfo = new();
        private readonly AddInfo _addInfo = new();
        private Coordinate? _lastOrthodromeCoordinate = new();
        private readonly RotateInfo _rotateInfo = new();
        private readonly ScaleInfo _scaleInfo = new();
        private const double LineStep = 0.5;

        public EditMode EditMode { get; set; }

        public int VertexRadius { get; set; } = 12;

        public bool EndEdit()
        {
            if (_addInfo.Feature is null) return false;
            if (_addInfo.Vertices is null) return false;

            if (EditMode == EditMode.DrawingLine)
            {
                _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // Remove duplicate last element added by the final double click
                //_addInfo.Feature.Geometry = new LineString(_addInfo.Vertices.ToArray()); // TODO: Bug when doubleclick immediately

                _addInfo.Feature = null;
                _addInfo.Vertex = null;
                EditMode = EditMode.AddLine;
            }
            else if (EditMode == EditMode.DrawingPolygon)
            {
                _addInfo.Vertices.RemoveAt(_addInfo.Vertices.Count - 1); // correct for double click
                var polygon = _addInfo.Feature.Geometry as Polygon;
                if (polygon == null) return false;

                _addInfo.Feature.LinearPoints = _addInfo.Vertices.ToList();

                _addInfo.Feature?.RenderedGeometry.Clear(); // You need to clear the cache to see changes.
                _addInfo.Feature = null;
                _addInfo.Vertex = null;
                EditMode = EditMode.AddPolygon;
                Layer?.DataHasChanged();
            }
            else if (EditMode == EditMode.DrawingOrthodromeLine)
            {
                //_addInfo.Feature.Geometry = new LineString(_addInfo.Vertices.ToArray());

                _addInfo.Feature = null;
                _addInfo.Vertex = null;
                EditMode = EditMode.AddOrthodromeLine;
            }

            return false;
        }

        internal void HoveringVertex(MapInfo? mapInfo)
        {
            if (_addInfo.Vertex != null)
            {
                _addInfo.Vertex.SetXY(mapInfo?.WorldPosition);
                _addInfo.Feature?.RenderedGeometry.Clear();
                Layer?.DataHasChanged();
            }
        }

        internal void HoveringOrthodronomeVertex(MapInfo? mapInfo)
        {
            if (_addInfo.Vertex != null)
            {
                _addInfo.Vertex.SetXY(mapInfo?.WorldPosition);
                if (_lastOrthodromeCoordinate != null)
                {
                    // change drawing
                    //_addInfo.Feature!.Geometry = new LineString(GetOrthodromeLonLatPath(_addInfo.Vertex, _lastOrthodromeCoordinate).LonLatToWorld().ToArray());
                    _addInfo.Feature?.RenderedGeometry.Clear();
                    Layer?.DataHasChanged();
                }
            }
        }

        public bool AddVertex(Coordinate worldPosition)
        {
            if (EditMode == EditMode.AddPoint)
            {
#pragma warning disable IDISP004 // Don't ignore created IDisposable
                Layer?.Add(new PointGraphic { LinearPoints = new[] { worldPosition } });
                Layer?.DataHasChanged();
#pragma warning restore IDISP004 // Don't ignore created IDisposable
            }
            else if (EditMode == EditMode.AddPolygon)
            {
                var firstPoint = worldPosition.Copy();
                // Add a second point right away. The second one will be the 'hover' vertex
                var secondPoint = worldPosition.Copy();
                _addInfo.Vertex = secondPoint;
                _addInfo.Vertices = new List<Coordinate>(new[] { firstPoint, secondPoint });
                _addInfo.Feature = new PolygonGraphic
                {
                    LinearPoints = _addInfo.Vertices.ToList()
                };
                Layer?.Add(_addInfo.Feature);
                Layer?.DataHasChanged();
                EditMode = EditMode.DrawingPolygon;
            }
            else if (EditMode == EditMode.DrawingPolygon)
            {
                if (_addInfo.Feature is null) return false;
                if (_addInfo.Vertices is null) return false;

                // Set the final position of the 'hover' vertex (that was already part of the geometry)
                _addInfo.Vertex.SetXY(worldPosition);
                _addInfo.Vertex = worldPosition.Copy(); // and create a new hover vertex
                _addInfo.Vertices.Add(_addInfo.Vertex);

                _addInfo.Feature.LinearPoints = _addInfo.Vertices.ToList();
                _addInfo.Feature?.RenderedGeometry.Clear();
                Layer?.DataHasChanged();
            }
            else if (EditMode == EditMode.AddOrthodromeLine)
            {
                _lastOrthodromeCoordinate = worldPosition.Copy();
                _addInfo.Vertex = worldPosition.Copy();
                //_addInfo.Feature = new OrthodromeGraphic { Geometry = new LineString(new[] { _addInfo.Vertex, _lastOrthodromeCoordinate }) };
                _addInfo.Vertices = _addInfo.Feature.Geometry.MainCoordinates();
                Layer?.Add(_addInfo.Feature);
                Layer?.DataHasChanged();
                EditMode = EditMode.DrawingOrthodromeLine;
            }
            else if (EditMode == EditMode.DrawingOrthodromeLine)
            {
                if (_addInfo.Feature is null) return false;
                if (_addInfo.Vertices is null) return false;

                _addInfo.Vertex.SetXY(worldPosition);
                _addInfo.Vertex = worldPosition.Copy();
                if (_lastOrthodromeCoordinate != null)
                {
                    foreach(var vertex in GetOrthodromeLonLatPath(_addInfo.Vertex, _lastOrthodromeCoordinate).LonLatToWorld())
                    {
                        _addInfo.Vertices.Add(vertex);
                    }
                    //_addInfo.Feature.Geometry = new LineString(_addInfo.Vertices.ToArray());
                    _lastOrthodromeCoordinate = _addInfo.Vertex.Copy();
                    _addInfo.Feature?.RenderedGeometry.Clear();
                    Layer?.DataHasChanged();
                }
            }

            return false;
        }

        private static List<Coordinate> GetOrthodromeLonLatPath(MPoint lonLatDeg1, MPoint lonLatDeg2)
        {
            var lat1Rad = Algorithms.DegreesToRadians(lonLatDeg1.Y);
            var lat2Rad = Algorithms.DegreesToRadians(lonLatDeg2.Y);
            var lon1Rad = Algorithms.DegreesToRadians(lonLatDeg1.X);
            var lon2Rad = Algorithms.DegreesToRadians(lonLatDeg2.X);

            var points = new List<Coordinate>();
            var left = Math.Min(lonLatDeg1.X, lonLatDeg2.X) == lonLatDeg1.X ? lonLatDeg1 : lonLatDeg2;
            var right = left.X == lonLatDeg1.X ? lonLatDeg2 : lonLatDeg1;
            for (var lon = left.X; lon <= right.X; lon += LineStep)
            {  
                var lonRad = Algorithms.DegreesToRadians(lon);
                var lat = Math.Atan((Math.Tan(lat1Rad) * Math.Sin(lon2Rad - lonRad)) / (Math.Sin(lon2Rad - lon1Rad)) + (Math.Tan(lat2Rad) * Math.Sin(lonRad - lon1Rad)) / (Math.Sin(lon2Rad - lon1Rad)));  
                points.Add(new Coordinate(lon, lat / Math.PI * 180.0));
            }
            points.Add(right.ToCoordinate());
            return points;
        }

        private static List<Coordinate> GetOrthodromeLonLatPath(Coordinate worldPoint1, Coordinate worldPoint2)
        {
            return GetOrthodromeLonLatPath(
                SphericalMercator.ToLonLat(worldPoint1.ToMPoint()),
                SphericalMercator.ToLonLat(worldPoint2.ToMPoint())
            );
        }

        private static Coordinate? FindVertexTouched(MapInfo mapInfo, IEnumerable<Coordinate> vertices, double screenDistance)
        {
            if (mapInfo.WorldPosition == null)
                return null;

            return vertices.OrderBy(v => v.Distance(mapInfo.WorldPosition.ToCoordinate()))
                .FirstOrDefault(v => v.Distance(mapInfo.WorldPosition.ToCoordinate()) < mapInfo.Resolution * screenDistance);
        }

        public bool StartDragging(MapInfo mapInfo, double screenDistance)
        {
            if (EditMode == EditMode.Modify)
            {
                if (mapInfo.Feature != null)
                {
                    if (mapInfo.Feature is BaseGraphic geometryFeature)
                    {
                        var vertexTouched = FindVertexTouched(mapInfo, geometryFeature.Geometry?.MainCoordinates() ?? new List<Coordinate>(), screenDistance);
                        if (vertexTouched != null)
                        {
                            _dragInfo.Feature = geometryFeature;
                            _dragInfo.Vertex = vertexTouched;
                            if (mapInfo.WorldPosition != null && _dragInfo.Vertex != null)
                            {
                                _dragInfo.StartOffsetToVertex = mapInfo.WorldPosition - _dragInfo.Vertex.ToMPoint();
                            }

                            return true; // to indicate start of drag
                        }
                    }
                }
            }
            return false;
        }

        public bool Dragging(Point? worldPosition)
        {
            if (EditMode != EditMode.Modify || _dragInfo.Feature == null || worldPosition == null || _dragInfo.StartOffsetToVertex == null) return false;

            _dragInfo.Vertex.SetXY(worldPosition.ToMPoint() - _dragInfo.StartOffsetToVertex);

            if (_dragInfo.Feature.Geometry is Polygon polygon) // Not this only works correctly it the feature is in the outer ring.
            {
                var count = polygon.ExteriorRing?.Coordinates.Length ?? 0;
                var vertices = polygon.ExteriorRing?.Coordinates ?? Array.Empty<Coordinate>();
                var index = vertices.ToList().IndexOf(_dragInfo.Vertex!);
                if (index >= 0)
                    // It is a ring where the first should be the same as the last.
                    // So if the first was removed than set the last to the value of the new first
                    if (index == 0) vertices[count - 1].SetXY(vertices[0]);
                    // If the last was removed then set the first to the value of the new last
                    else if (index == vertices.Length) vertices[0].SetXY(vertices[count - 1]);
            }

            _dragInfo.Feature.RenderedGeometry.Clear();
            Layer?.DataHasChanged();
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
                        Layer?.DataHasChanged();
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
                    geometryFeature.Geometry = geometryFeature.Geometry.InsertCoordinate(mapInfo.WorldPosition.ToCoordinate(), segment);
                    geometryFeature.RenderedGeometry.Clear();
                    Layer?.DataHasChanged();
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
            Layer?.DataHasChanged();

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
            Layer?.DataHasChanged();

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
    }
}