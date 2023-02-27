using map_app.Services;

namespace map_app.Models.Extensions;

public class Segment
{
    public GeoPoint Start { get; set; }
    public GeoPoint End { get; set; }
    public double Distance => MapAlgorithms.Haversine(Start, End);

    public Segment(GeoPoint start, GeoPoint end)
    {
        Start = start;
        End = end;
    }
}