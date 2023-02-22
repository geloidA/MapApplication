using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace map_app.Models;

public interface IHoveringGraphic
{
    public Coordinate? HoverVertex { get; set; }
    void AddPoint(Coordinate worldCoordinate);
}