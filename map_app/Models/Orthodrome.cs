using System.Collections.Generic;
using map_app.Services;

namespace map_app.Models
{
    public class Orthodrome
    {
        private List<GeoPoint> _path;

        public Orthodrome(GeoPoint start, GeoPoint end)
        {
            Start = start;
            End = end;
            _path = MapAlgorithms.GetOrthodromePath(Start, End);
        }

        public GeoPoint Start { get; set; }

        public GeoPoint End { get; set; }

        public List<GeoPoint> Path => _path;

        /// <summary>
        /// Calculate great circle path
        /// </summary>
        public void RenderPath()
        {
            _path = MapAlgorithms.GetOrthodromePath(Start, End);
        }

        public override string ToString()
        {
            return string.Format("Start:{0}\nEnd:{1}", Start, End);
        }
    }
}