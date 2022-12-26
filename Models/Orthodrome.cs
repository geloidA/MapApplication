using System.Collections.Generic;
using map_app.Services;

namespace map_app.Models
{
    public class Orthodrome
    {
        private List<GeoPoint> _path;
        private GeoPoint _start;
        private GeoPoint _end;

        public Orthodrome(GeoPoint start, GeoPoint end)
        {
            _start = start;
            _end = end;
            _path = MapAlgorithms.GetOrthodromePath(start, end);
        }

        public Orthodrome? Next { get; set; }

        public GeoPoint Start
        {
            get => _start;
            set
            {
                _start = value;
                RenderValue();
            }
        }

        public GeoPoint End
        { 
            get => _end;
            set
            {
                _end = value;
                RenderValue();
            }
        }

        public List<GeoPoint> Path => _path;

        private void RenderValue()
        {
            _path = MapAlgorithms.GetOrthodromePath(Start, End);
        }
    }
}