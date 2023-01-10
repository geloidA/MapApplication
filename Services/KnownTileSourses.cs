using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BruTile.Predefined;
using BruTile.Web;

namespace map_app.Services
{
    public static class DictKnownTileSources
    {
        private static Dictionary<string, KnownTileSource> domenMasks = new()
        {
            {"www.openstreetmap.org", KnownTileSource.OpenStreetMap }
        };

        public static HttpTileSource CreateViaDomen(string domen)
        {
            if (!domenMasks.ContainsKey(domen))
                throw new NotSupportedException($"domen is not supported");
            var httpSourse = KnownTileSources.Create(domenMasks[domen]);
            httpSourse.Attribution = new BruTile.Attribution(url: domen);
            return httpSourse;
        }

        public static HttpTileSource CreateViaMask(string mask)
        {
            return new HttpTileSource(new GlobalSphericalMercator(), mask);
        }
    }
}