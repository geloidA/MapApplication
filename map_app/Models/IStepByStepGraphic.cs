using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace map_app.Models;

public interface IStepByStepGraphic
{
    void AddPoint(Coordinate worldCoordinate);
    void AddRangePoints(IEnumerable<Coordinate> worldCoordinates);
    void RemoveHoverVertex();
}