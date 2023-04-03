using map_app.Services;
using System.Collections.Generic;

namespace map_app.Models;

public class Orthodrome
{
    public Orthodrome(GeoPoint start)
    {
        Start = start;
    }

    public Orthodrome(GeoPoint start, GeoPoint end)
    {
        Start = start;
        End = end;
    }

    public GeoPoint Start { get; set; }

    public GeoPoint? End { get; set; }

    public IEnumerable<GeoPoint> Path => MapAlgorithms.GetOrthodromePath(Start, End ?? Start);

    public override string ToString() => $"Start:{Start} End:{End}";
}