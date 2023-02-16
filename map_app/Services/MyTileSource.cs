using System.Collections.Generic;
using BruTile.Predefined;
using BruTile.Web;

namespace map_app.Services
{
    public static class MyTileSource
    {
        private static Dictionary<string, KnownTileSource> domenMasks = new()
        {
            {"www.openstreetmap.org", KnownTileSource.OpenStreetMap }
        };

        public static HttpTileSource Create(string s)
        {
            return domenMasks.ContainsKey(s)
                ? CreateViaDomen(s)
                : CreateViaMask(s);
        }

        private static HttpTileSource CreateViaDomen(string domen)
        {
            var httpSourse = KnownTileSources.Create(domenMasks[domen]);
            httpSourse.Attribution = new BruTile.Attribution(url: domen);
            return httpSourse;
        }

        private static HttpTileSource CreateViaMask(string mask)
        {
            var sourse = new HttpTileSource(new GlobalSphericalMercator(), mask);
            sourse.Attribution = new BruTile.Attribution(url: mask);
            return sourse;
        }
    }
}