using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public interface IStepByStepGraphic
    {
        void AddLinearPoint(Coordinate worldCoordinate);
        void AddRangeLinearPoints(IEnumerable<Coordinate> worldCoordinates);
        Geometry RenderStepGeometry(Coordinate worldPosition);
    }
}