using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using map_app.Models;
using map_app.Models.Extensions;
using System.Linq;
using Mapsui.Styles;
using Newtonsoft.Json.Linq;

namespace map_app.Services
{
    public static class GraphicSerializer
    {
        public static string Serialize(BaseGraphic graphic)
        {
            return JsonConvert.SerializeObject(graphic);
        }

        public static string SerializeCollection(IEnumerable<BaseGraphic> graphics)
        {
            return JsonConvert.SerializeObject(graphics, Formatting.Indented);
        }

        public static BaseGraphic Deserialize(string json)
        {
            dynamic? jsonGraphic = JsonConvert.DeserializeObject(json);

            if (jsonGraphic is null)
                throw new NullReferenceException("Deserialized object was null");

            BaseGraphic graphic;
            List<LinearPoint> points = jsonGraphic.LinearPoints.ToObject<List<LinearPoint>>();

            graphic = (GraphicType)jsonGraphic.Type switch
            {
                GraphicType.Orthodrome => new OrthodromeGraphic(points.ToCoordinates().ToList()),
                GraphicType.Point => new PointGraphic(points.ToCoordinates().ToList()) { Image = jsonGraphic.Image.ToObject<string?>() },
                GraphicType.Rectangle => new RectangleGraphic(points.ToCoordinates().ToList()),
                GraphicType.Polygon => new PolygonGraphic(points.ToCoordinates().ToList()),
                _ => throw new NotImplementedException()
            };
            InitCommonProperties(jsonGraphic, graphic);
            return graphic;
        }

        private static void InitCommonProperties(dynamic jsonGraphic, BaseGraphic graphic)
        {
            graphic.Color = jsonGraphic.Color.ToObject<Color?>();
            graphic.UserTags = JsonConvert.DeserializeObject<Dictionary<string, string>>(((JToken)jsonGraphic.UserTags).ToString());
            graphic.Opacity = jsonGraphic.Opacity.ToObject<double>();
        }
    }
}