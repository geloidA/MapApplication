using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using map_app.Services;

namespace map_app.Models
{
    public class Orthodrome
    {
        private List<GeoPoint>? _value;

        public Orthodrome? Next { get; set; }
        public GeoPoint? Start { get; set; }
        public GeoPoint? End { get; set; }
        public List<GeoPoint> Value
        {
            get
            {
                if (_value is null)
                    _value = MapAlgorithms.GetOrthodromePath(Start, End);
                return _value;
            }
        }
    }
}