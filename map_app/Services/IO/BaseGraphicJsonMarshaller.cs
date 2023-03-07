using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using map_app.Models;
using Newtonsoft.Json;

namespace map_app.Services.IO
{
    public class BaseGraphicJsonMarshaller
    {
        public static async Task<bool> TryLoadAsync(List<BaseGraphic> target, string loadLocation) // todo: refactor via IAsyncEnumerable
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

        public static async Task SaveAsync(MapState state, string saveLocation)
        {
            var graphicsString = JsonConvert.SerializeObject(state);
            using (var writer = new StreamWriter(saveLocation, false))
            {
                await writer.WriteLineAsync(graphicsString);
            }
        }

        private static bool TryDeserialize(string json, out BaseGraphic? graphic)
        {
            graphic = null;
            try
            {
                graphic = GraphicSerializer.Deserialize(json);
            }
            catch { return false; }            
            return true;
        }
    }
}