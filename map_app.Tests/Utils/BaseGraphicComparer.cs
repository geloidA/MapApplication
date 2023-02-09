using System.Collections;
using map_app.Models;
using map_app.Services;

namespace map_app.Tests.Utils
{
    public class BaseGraphicComparer : IComparer
    {
        private const double Eps = 1e-5;
        public int Compare(object? obj1, object? obj2)
        {
            if (obj1 is not BaseGraphic x ||
                obj2 is not BaseGraphic y) throw new ArgumentException("Values is not BaseGraphic");
            if (Equals(x, y)) return 0;
            throw new NotImplementedException();
        }

        private bool Equals(BaseGraphic x, BaseGraphic y)
        {
            var linearPointEquals = x.LinearPoints.SequenceEqual(y.LinearPoints, 
                new ThreeDimentionalPointEqualityComparer());
            var geoPointEquals = x.GeoPoints.SequenceEqual(y.GeoPoints, 
                new ThreeDimentionalPointEqualityComparer());                
            var tagsEquals = x.UserTags?.Count == y.UserTags?.Count 
                && DictionaryEquals(x.UserTags, y.UserTags);
            return x.Type == y.Type 
                && x.StyleColor.Equals(y.StyleColor)
                && linearPointEquals
                && geoPointEquals
                && tagsEquals
                && Math.Abs(x.Opacity - y.Opacity) < Eps;
        }

        private bool DictionaryEquals(Dictionary<string, string>? x, Dictionary<string, string>? y) 
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return !x.Except(y ?? new Dictionary<string, string>()).Any();
        }
    }
}