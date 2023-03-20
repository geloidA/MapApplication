using map_app.Models;
using Mapsui;

namespace map_app.Editing
{
    public class DragInfo
    {
        public BaseGraphic? Feature { get; set; }
        public MPoint[]? StartOffsetsToVertexes { get; set; }
    }
}