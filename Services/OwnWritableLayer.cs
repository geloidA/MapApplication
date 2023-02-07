using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Styles;

namespace map_app.Services
{
    public class OwnWritableLayer : WritableLayer, IEnumerable<IFeature>
    {
        private readonly List<IFeature> orderedFeatures = new();

        public new void Add(IFeature feature)
        {
            base.Add(feature);
            orderedFeatures.Add(feature);
            OnLayersFeatureChanged();
        }

        public override IEnumerable<IFeature> GetFeatures(MRect? box, double resolution) // for correct drawing "always on top"
        {
            // Safeguard in case BoundingBox is null, most likely due to no features in layer
            if (box == null) return new List<IFeature>();
            var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            var result = orderedFeatures.Where(f => biggerBox.Intersects(f.Extent));
            return result;
        }

        public new void AddRange(IEnumerable<IFeature> features)
        {
            base.AddRange(features);
            orderedFeatures.AddRange(features);
            OnLayersFeatureChanged();
        }

        public new bool TryRemove(IFeature feature, Func<IFeature, IFeature, bool>? compare = null)
        {
            var success = base.TryRemove(feature, compare);
            orderedFeatures.Remove(feature); // todo: 
            if (success)
            {
                feature.Dispose();
                OnLayersFeatureChanged();
            }
            return success;
        }

        public new void Clear()
        {
            base.Clear();
            orderedFeatures.Clear();
        }

        /// <summary>
        ///  Include DataHasChanged call
        /// </summary> 
        public void LayersFeatureHasChanged()
        {
            OnLayersFeatureChanged();
            base.DataHasChanged();
        }

        public event DataChangedEventHandler? LayersFeatureChanged;

        private void OnLayersFeatureChanged()
        {
            LayersFeatureChanged?.Invoke(this, new DataChangedEventArgs());
        }

        public IEnumerator<IFeature> GetEnumerator()
        {
            return orderedFeatures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}