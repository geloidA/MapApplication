using System.Collections.Generic;

namespace map_app.Services;

public class ConfigurationData
{
    public int DeliveryPort { get; set; }
    public IEnumerable<TileSource>? TileSources { get; set; }
}