using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using map_app.Models;
using Mapsui.Layers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace map_app.Services.IO
{
    public class BaseGraphicJsonMarshaller
    {
        internal static async Task LoadAsync(OwnWritableLayer target, string loadLocation) // refactor
        {
            var graphics = new List<BaseGraphic>();
            string json;
            using (var reader = new StreamReader(loadLocation))
            {
                json = await reader.ReadToEndAsync();
            }
            var objects = JsonConvert.DeserializeObject<JArray>(json) ?? throw new FileLoadException();
            foreach (var graphic in objects)
                graphics.Add(GraphicSerializer.Deserialize(graphic.ToString()) as BaseGraphic);
            target.AddRange(graphics);
        }

        internal static async Task SaveAsync(IEnumerable<BaseGraphic> graphics, string saveLocation)
        {
            var graphicsString = GraphicSerializer.SerializeCollection(graphics);
            using (var writer = new StreamWriter(saveLocation, false))
            {
                await writer.WriteAsync(graphicsString);
            }
        }
    }
}