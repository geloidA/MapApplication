using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using map_app.Models;

namespace map_app.Services
{
    public class GeoPointsEqualityComparer : EqualityComparer<GeoPoint>
    {
        private const double Eps = 1e-5;

        public override bool Equals(GeoPoint? x, GeoPoint? y)
        {
            if (x is null || y is null) throw new NotImplementedException();
            if (ReferenceEquals(x, y)) return true;

            return Math.Abs(x.Longtitude - y.Longtitude) < Eps
                && Math.Abs(x.Latitude - y.Latitude) < Eps
                && Math.Abs(x.Altitude - y.Altitude) < Eps;
        }

        public override int GetHashCode([DisallowNull] GeoPoint obj)
        {
            return (obj.Longtitude.GetHashCode() * 31 + obj.Latitude.GetHashCode()) 
                * 31 + obj.Altitude.GetHashCode();
        }
    }
}