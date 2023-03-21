using System.IO;
using System.Threading.Tasks;

namespace map_app.Services.IO;

public class MapStateJsonMarshaller
{
    public static async Task<MapState?> LoadAsync(string loadLocation)
    {
        return MapStateJsonSerializer.Deserialize(await File.ReadAllTextAsync(loadLocation));
    }

    public static async Task SaveAsync(MapState state, string saveLocation)
    {
        var graphicsString = MapStateJsonSerializer.Serialize(state);
        using (var writer = new StreamWriter(saveLocation, false))
        {
            await writer.WriteLineAsync(graphicsString);
        }
    }
}