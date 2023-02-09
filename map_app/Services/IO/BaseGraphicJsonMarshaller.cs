using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using map_app.Models;

namespace map_app.Services.IO
{
    public class BaseGraphicJsonMarshaller
    {
        public static async Task LoadAsync(OwnWritableLayer target, string loadLocation)
        {
            var graphics = new List<BaseGraphic>();
            using (var reader = new StreamReader(loadLocation))
            {
                string? json;
                while ((json = await reader.ReadLineAsync()) != null)
                    graphics.Add(GraphicSerializer.Deserialize(json));
            }
            target.AddRange(graphics);
        }

        public static async Task SaveAsync(IEnumerable<BaseGraphic> graphics, string saveLocation)
        {
            var graphicsString = GraphicSerializer.Serialize(graphics);
            using (var writer = new StreamWriter(saveLocation, false))
            {
                foreach (var graphic in graphicsString)
                    await writer.WriteLineAsync(graphic);
            }
        }
    }
}