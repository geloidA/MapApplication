using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using System.Threading.Tasks;
using Mapsui.Fetcher;

namespace map_app.Models
{
    public record Aircraft(string Id, double Longtitude, double Latitude, double Height);
}