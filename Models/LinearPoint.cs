using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class LinearPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public LinearPoint() : this(0, 0, 0) { }
        public LinearPoint(double x, double y) : this(x, y, 0) { }

        public LinearPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Coordinate ToCoordinate()
        {
            return new Coordinate3D(X, Y, Z);
        }

        public override string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z}";
        }

        public override bool Equals(object? obj)
        {
            var other = obj as LinearPoint;
            if (other == null)
                return false;
            
            return X == other.X
                && Y == other.Y
                && Z == other.Z;
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}