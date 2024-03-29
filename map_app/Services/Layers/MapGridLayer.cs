using Mapsui;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace map_app.Services.Layers;

public class MapGridLayer : BaseLayer
{
    private readonly GridMemoryProvider _dataSource;
    private ImmutableArray<IFeature> _gridLines;

    public MapGridLayer(GridMemoryProvider dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentException("dataSource was null", nameof(dataSource));
        _gridLines = ImmutableArray.Create<IFeature>();
        if (_dataSource is IDynamic dynamic)
            dynamic.DataChanged += (s, e) =>
            {
                Catch.Exceptions(async () => { if (Enabled) await UpdateDataAsync(); });
            };
    }

    public async Task UpdateDataAsync()
    {
        var features = await _dataSource.GetFeaturesAsync(null!);
        _gridLines = ImmutableArray.CreateRange(features);
        OnDataChanged(new DataChangedEventArgs());
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        foreach (var line in _gridLines)
            yield return line;
    }
}