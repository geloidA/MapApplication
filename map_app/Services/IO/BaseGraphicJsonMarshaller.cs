using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using map_app.Models;

namespace map_app.Services.IO
{
    public class BaseGraphicJsonMarshaller
    {
        public static async Task<bool> TryLoadAsync(List<BaseGraphic> target, string loadLocation)
        {
            using (var reader = new StreamReader(loadLocation))
            {
                string? json;
                while ((json = await reader.ReadLineAsync()) != null)
                {
                    if (TryDeserialize(json, out BaseGraphic? graphic))
                        target.Add(graphic!);
                    else
                        return false;
                }
            }
            return true;
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

        private static bool TryDeserialize(string json, out BaseGraphic? graphic)
        {
            graphic = null;
            try
            {
                graphic = GraphicSerializer.Deserialize(json);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}