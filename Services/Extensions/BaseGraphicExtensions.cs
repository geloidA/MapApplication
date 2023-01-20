using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using map_app.Models;
using Mapsui;

namespace map_app.Services.Extensions
{
    public static class BaseGraphicExtensions
    {
        public static IEnumerable<IFeature> Copy(this IEnumerable<BaseGraphic> original)
        {
            foreach (var graphic in original)
                yield return graphic.LightCopy();
        }
    }
}