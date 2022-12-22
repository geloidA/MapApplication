using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class OrthodromeGraphic : BaseGraphic
    {        
        public OrthodromeGraphic() : base() { }
        public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public OrthodromeGraphic(Geometry? geometry) : base(geometry) { }
        
        protected override Geometry ConstructGeomerty(List<Coordinate> points)
        {
            throw new NotImplementedException();
        }
    }
}