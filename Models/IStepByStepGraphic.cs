using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public interface IStepByStepGraphic
    {
        void AddPoint(Coordinate worldCoordinate);
        void AddRangePoints(IEnumerable<Coordinate> worldCoordinates);
        Geometry RenderStepGeometry(Coordinate worldPosition);
    }
}