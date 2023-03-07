using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;

namespace map_app.Services;

public class MapGridLayer : BaseLayer
{
    private GridMemoryProvider _dataSource;
    private readonly List<IFeature> _gridLines;

    public double KilometerInterval { get; set; }

    public MapGridLayer(GridMemoryProvider dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentException(nameof(dataSource));
        _gridLines = new List<IFeature>();
        if (_dataSource is IDynamic dynamic)
            dynamic.DataChanged += (s, e) => { 
                Catch.Exceptions(async () => await UpdateDataAsync());
            };
    }

    public async Task UpdateDataAsync()
    {
        var features = await _dataSource.GetFeaturesAsync(new FetchInfo(new MRect(0, 0, 0, 0), 0));
        _gridLines.Clear();
        _gridLines.AddRange(features);
        OnDataChanged(new DataChangedEventArgs());
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        return _gridLines;
    }
}