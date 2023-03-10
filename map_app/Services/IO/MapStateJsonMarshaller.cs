using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace map_app.Services.IO
{
    public class MapStateJsonMarshaller
    {
        public static async Task<MapState?> LoadAsync(string loadLocation)
        {
            return JsonConvert.DeserializeObject<MapState>(await File.ReadAllTextAsync(loadLocation));
        }

        public static async Task SaveAsync(MapState state, string saveLocation)
        {
            var graphicsString = JsonConvert.SerializeObject(state);
            using (var writer = new StreamWriter(saveLocation, false))
            {
                await writer.WriteLineAsync(graphicsString);
            }
        }
    }
}