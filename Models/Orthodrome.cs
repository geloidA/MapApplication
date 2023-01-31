using System;
using System.Collections.Generic;
using System.Linq;
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
            _path = MapAlgorithms.GetOrthodromePath(_start, _end);
        }

        public Orthodrome? Next { get; set; }

        /// <summary>
        ///  Set call renders Path
        /// </summary>
        public GeoPoint Start
        {
            get => _start;
            set
            {
                _start = value;
                RenderValue();
            }
        }

        /// <summary>
        ///  Set call renders Path
        /// </summary>
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

        public override string ToString()
        {
            return string.Format("Start:{0}\nEnd:{1}", Start, End);
        }

        public static Orthodrome Create(List<GeoPoint> points)
        {
            if (points is null) throw new NullReferenceException();
            if (points.Count < 2) throw new ArgumentException("Points count must be bigger then 2");
            var head = new Orthodrome(points[0], points[1]);
            var current = head;
            foreach (var point in points.Skip(2))
            {
                current.Next = new Orthodrome(current.End, point);
                current = current.Next;
            }
            return head;
        }
    }
}