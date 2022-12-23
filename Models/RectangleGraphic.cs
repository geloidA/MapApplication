using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class RectangleGraphic : BaseGraphic
    {
        public RectangleGraphic() : base() { }
        public RectangleGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public RectangleGraphic(Geometry? geometry) : base(geometry) { }

        protected override Geometry RenderGeomerty(List<Coordinate> points)
        {
            throw new NotImplementedException();
        }
    }
}