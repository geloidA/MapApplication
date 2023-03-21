using System.Collections.Generic;
using System.Linq;
using map_app.Models;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;

namespace map_app.Services.Layers;

public class GraphicsLayer : BaseLayer
{
    private readonly List<BaseGraphic> _graphics = new();

    public IEnumerable<BaseGraphic> Features 
    {
        get
        {
            foreach (var graphic in _graphics)
                yield return graphic;
        }
    }

    public void Add(BaseGraphic graphic)
    {
        _graphics.Add(graphic);
        OnLayersFeatureChanged(CollectionOperation.Add, new[] { graphic });
    }

    public override IEnumerable<IFeature> GetFeatures(MRect? box, double resolution) // for correct graphics order drawing
    {
        if (box == null) return new List<IFeature>();
        var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
        var result = _graphics.Where(f => biggerBox.Intersects(f.Extent));
        return result;
    }

    public void AddRange(IEnumerable<BaseGraphic> features)
    {
        _graphics.AddRange(features);
        OnLayersFeatureChanged(CollectionOperation.AddRange, features);
    }

    public bool TryRemove(BaseGraphic graphic)
    {
        var success = _graphics.Remove(graphic);
        if (success)
        {
            graphic.Dispose();
            OnLayersFeatureChanged(CollectionOperation.Remove, new[] { graphic });
        }
        return success;
    }

    public void Clear()
    {
        foreach (var feature in _graphics)
            feature.Dispose();
        _graphics.Clear();
        OnLayersFeatureChanged(CollectionOperation.Clear, Enumerable.Empty<BaseGraphic>());
    }

    public override void Dispose()
    {
        base.Dispose();
        Clear();
    }

    public event MDataChangedEventHandler? LayersFeatureChanged;

    private void OnLayersFeatureChanged(CollectionOperation operation, IEnumerable<BaseGraphic> graphics) 
        => LayersFeatureChanged?.Invoke(this, new MDataChangedEventArgs(operation, graphics));        
}