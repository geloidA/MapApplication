using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using map_app.Models;

namespace map_app.Services
{
    public class ThreeDimentionalPointEqualityComparer : EqualityComparer<IThreeDimensionalPoint>
    {
        private const double Eps = 1e-5;

        public override bool Equals(IThreeDimensionalPoint? x, IThreeDimensionalPoint? y)
        {
            if (x is null || y is null) throw new NotImplementedException();
            if (ReferenceEquals(x, y)) return true;

            return Math.Abs(x.First - y.First) < Eps
                && Math.Abs(x.Second - y.Second) < Eps
                && Math.Abs(x.Third - y.Third) < Eps;
        }

        public override int GetHashCode([DisallowNull] IThreeDimensionalPoint obj)
        {
            return (obj.First.GetHashCode() * 31 + obj.Second.GetHashCode()) 
                * 31 + obj.Third.GetHashCode();
        }
    }
}