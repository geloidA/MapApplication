using System.Collections.Generic;
using System.Text;
using map_app.Models;
using Newtonsoft.Json;

namespace map_app.Services;

[JsonObject(MemberSerialization.OptIn)]
public class MapState
{
    [JsonProperty]
    public string? Name { get; set; }

    [JsonProperty]
    public string? Description { get; set; }

    [JsonProperty]
    public IEnumerable<BaseGraphic>? Graphics { get; set; }

    public bool IsInitialized => Name != null || Description != null || Graphics != null;
    
    public string FileLocation { get; set; } = string.Empty;

    public byte[] ToJsonBytes()
    {
        var json = MapStateJsonSerializer.Serialize(this);
        return Encoding.UTF8.GetBytes(json);
    }
}