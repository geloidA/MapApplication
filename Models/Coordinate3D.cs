using System;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class Coordinate3D : Coordinate
    {
        private double _z;

        public override double Z { get => _z; set => _z = value; }

        public Coordinate3D()
        {
            X = 0; Y = 0; Z = 0;
        }

        public Coordinate3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override Coordinate CoordinateValue 
        { 
            get => this; 
            set
            {
                if (value is Coordinate3D coordinate)
                {
                    X = coordinate.X;
                    Y = coordinate.Y;
                    Z = coordinate.Z;
                }
                else
                    throw new ArgumentException("Coordinate isn't a Coordinate3D");
            }
        }

        public override Coordinate Copy() => new Coordinate3D(X, Y, Z);
    }
}