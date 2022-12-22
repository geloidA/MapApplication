using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class RectangleGraphic : BaseGraphic
    {
        protected override Geometry ConstructGeomerty(List<Coordinate> points)
        {
            throw new NotImplementedException();
        }
    }
}