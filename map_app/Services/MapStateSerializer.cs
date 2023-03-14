using Newtonsoft.Json;

namespace map_app.Services;

public static class MapStateJsonSerializer
{
    public static string Serialize(MapState state) => JsonConvert.SerializeObject(state);

    public static MapState? Deserialize(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<MapState>(json);
        }
        catch (JsonReaderException)
        {
            return null;
        }
    }
}