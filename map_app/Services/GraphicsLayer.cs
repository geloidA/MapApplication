using System.Collections.Generic;
using System.Linq;
using map_app.Models;
using Mapsui;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Styles;

namespace map_app.Services
{
    public class GraphicsLayer : BaseLayer
    {
        private readonly List<BaseGraphic> _graphics = new();

        public IEnumerable<BaseGraphic> Features 
        {
            get
            {
                foreach (var graphic in _graphics)
                {
                    yield return graphic;
                }
            }
        }

        public void Add(BaseGraphic graphic)
        {
            _graphics.Add(graphic);
            OnLayersFeatureChanged();
        }

        public override IEnumerable<IFeature> GetFeatures(MRect? box, double resolution) // for correct drawing "always on top"
        {
            if (box == null) return new List<IFeature>();
            var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            var result = _graphics.Where(f => biggerBox.Intersects(f.Extent));
            return result;
        }

        public void AddRange(IEnumerable<BaseGraphic> features)
        {
            _graphics.AddRange(features);
            OnLayersFeatureChanged();
        }

        public bool TryRemove(BaseGraphic graphic)
        {
            var success = _graphics.Remove(graphic);
            if (success)
            {
                graphic.Dispose();
                OnLayersFeatureChanged();
            }
            return success;
        }

        public void Clear()
        {
            foreach (var feature in _graphics)
                feature.Dispose();
            _graphics.Clear();
        }

        /// <summary>
        ///  Include DataHasChanged call
        /// </summary> 
        public void LayersFeatureHasChanged()
        {
            OnLayersFeatureChanged();
            DataHasChanged();
        }

        public override void Dispose()
        {
            base.Dispose();
            Clear();
        }

        public event DataChangedEventHandler? LayersFeatureChanged;

        private void OnLayersFeatureChanged() => LayersFeatureChanged?.Invoke(this, new DataChangedEventArgs());        
    }
}