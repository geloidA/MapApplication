using System;
using Newtonsoft.Json;
using map_app.Models;

namespace map_app.Services
{
    public static class GraphicSerializer
    {
        public static string Serialize(BaseGraphic graphic)
        {
            return JsonConvert.SerializeObject(graphic);
        }

        public static BaseGraphic Deserialize(string json)
        {
            throw new NotImplementedException();
        }
    }
}