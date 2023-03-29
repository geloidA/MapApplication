using map_app.Models;
using NetTopologySuite.Geometries;

namespace map_app.Editing;

public class AddInfo
{
    public BaseGraphic? Feature { get; set; }
    public Coordinate? CurrentVertex { get; set; }
}