using map_app.Services;

namespace map_app.Models.Extensions;

public class Segment
{
    public GeoPoint Start { get; }
    public GeoPoint End { get; }
    public double Distance { get; }

    public Segment(GeoPoint start, GeoPoint end)
    {
        Start = start;
        End = end;
        Distance = MapAlgorithms.Haversine(Start, End);
    }
}