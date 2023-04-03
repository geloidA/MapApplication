using NetTopologySuite.Geometries;
using System;

namespace map_app.Models;

public class Coordinate3D : Coordinate
{
    public override double Z { get; set; }

    public Coordinate3D()
    {
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