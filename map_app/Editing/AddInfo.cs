using System.Collections.Generic;
using map_app.Models;
using NetTopologySuite.Geometries;

namespace map_app.Editing;

public class AddInfo
{
    public BaseGraphic? Feature { get; set; }
    public IList<Coordinate>? Vertices { get; set; }
    public Coordinate? Vertex { get; set; }
}