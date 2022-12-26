using map_app.Models;
using Mapsui;
using NetTopologySuite.Geometries;

namespace map_app.Editing
{
    public class DragInfo
    {
        public BaseGraphic? Feature { get; set; }
        public Coordinate? Vertex { get; set; }
        public MPoint? StartOffsetToVertex { get; set; }
    }
}