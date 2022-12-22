using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using map_app.Models;
using NetTopologySuite.Geometries;

namespace map_app.Editing
{
    public class AddInfo
    {
        public BaseGraphic? Feature;
        public IList<Coordinate>? Vertices;
        public Coordinate? Vertex;
    }
}