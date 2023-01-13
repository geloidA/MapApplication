using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Fetcher;
using Mapsui.Layers;

namespace map_app.Services
{
    public class OwnWritableLayer : WritableLayer
    {
        public new void Add(IFeature feature)
        {
            base.Add(feature);
            OnLayersFeatureChanged();
        }

        public new void AddRange(IEnumerable<IFeature> features)
        {
            base.AddRange(features);
            OnLayersFeatureChanged();
        }

        public new bool TryRemove(IFeature feature, Func<IFeature, IFeature, bool>? compare = null)
        {
            var success = base.TryRemove(feature, compare);
            if (success)
                OnLayersFeatureChanged();
            return success;
        }

        public event DataChangedEventHandler? LayersFeatureChanged;

        private void OnLayersFeatureChanged()
        {
            LayersFeatureChanged?.Invoke(this, new DataChangedEventArgs());
        }
    }
}