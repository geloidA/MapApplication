using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using map_app.Models;
using NetTopologySuite.Geometries;

namespace map_app.Editing
{
    public class ScaleInfo
    {
        public BaseGraphic? Feature { get; set; }
        public Point? PreviousPosition { get; set; }
        public Point? Center { get; set; }        
    }
}