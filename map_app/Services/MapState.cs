using System.Collections.Generic;
using map_app.Models;

namespace map_app.Services;

public class MapState
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<BaseGraphic>? Graphics { get; set; }
}