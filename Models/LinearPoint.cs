using System;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class LinearPoint
    {
        private const double Eps = 1e-5;

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
            if (obj is not LinearPoint other)
                return false;
            
            return Math.Abs(other.X - X) < Eps
                && Math.Abs(other.Y - Y) < Eps
                && Math.Abs(other.Z - Z) < Eps;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() * 31 + Y.GetHashCode()) * 31 + Z.GetHashCode();
        }
    }
}