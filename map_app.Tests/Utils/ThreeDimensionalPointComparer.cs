using System.Collections;
using map_app.Models;
using map_app.Services;

namespace map_app.Tests.Utils;

public class ThreeDimensionalPointComparer : IComparer
{
    EqualityComparer<IThreeDimensionalPoint> comparer = new ThreeDimentionalPointEqualityComparer();

    public int Compare(object? obj1, object? obj2)
    {
        if (obj1 is not IThreeDimensionalPoint x ||
            obj2 is not IThreeDimensionalPoint y) throw new ArgumentException("Values is not IThreeDimentionalPoint");
        if (comparer.Equals(x, y)) return 0;
        throw new NotImplementedException();
    }
}