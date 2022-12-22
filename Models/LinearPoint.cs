using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class LinearPoint : Coordinate
    {
        public LinearPoint() : this(0, 0, 0)
        {
        }

        public LinearPoint(double x, double y) : this(x, y, 0)
        {
        }

        public LinearPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}