using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;

namespace map_app.Editing.Extensions
{
    public static class GeometryExtensions
    {
        /// <summary>
        /// For editing features it is simpler if we can treat all
        /// geometries as a list of lists of points.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static IList<IList<Coordinate>> GetVertexLists(this Geometry geometry)
        {
            if (geometry is Point point)
            {
                return new List<IList<Coordinate>> { new List<Coordinate> { point.Coordinate } };
            }
            if (geometry is LineString lineString)
            {
                return new List<IList<Coordinate>> { new List<Coordinate>(lineString.Coordinates) };
            }
            if (geometry is Polygon polygon)
            {
                var lists = new List<IList<Coordinate>>
                {
                    polygon.ExteriorRing?.Coordinates.ToList() ?? new List<Coordinate>()
                };
                lists.AddRange(polygon.InteriorRings.Select(i => i.Coordinates));
                return lists;
            }
            throw new NotImplementedException();
        }

        public static List<Coordinate> MainCoordinates(this Geometry geometry)
        {
            if (geometry is LineString lineString)
                return lineString.Coordinates.ToList();
            if (geometry is Polygon polygon)
                return polygon.ExteriorRing?.Coordinates.ToList() ?? new List<Coordinate>();
            if (geometry is Point point)
                return new List<Coordinate> { point.Coordinate };
            throw new NotImplementedException();
        }
        
        public static Geometry? InsertCoordinate(this Geometry? geometry, Coordinate coordinate, int segment)
        {
            if (geometry is null)
                return null;

            var vertices = geometry.MainCoordinates();
            vertices.Insert(segment + 1, coordinate);

            if (geometry is Polygon)
                return vertices.ToPolygon();
            else if (geometry is LineString)
                return vertices.ToLineString();
            else
                throw new NotSupportedException();
        }

        public static Geometry? DeleteCoordinate(this Geometry? geometry, int index)
        {
            if (geometry is null)
                return null;

            var vertices = geometry.MainCoordinates();
            vertices.RemoveAt(index);

            if (geometry is Polygon)
                return vertices.ToPolygon();
            else if (geometry is LineString)
                return vertices.ToLineString();
            else
                throw new NotSupportedException();
        }
    }
}