using System.ComponentModel;

namespace map_app.Models
{
    public enum PointType
    {
        [Description("Линейные")]
        Linear,
        [Description("Угловые")]
        Geo
    }
}