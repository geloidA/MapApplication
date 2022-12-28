using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using map_app.Models;
using map_app.Models.Extensions;
using System.Linq;
using Mapsui.Styles;

namespace map_app.Services
{
    public static class GraphicSerializer
    {
        public static string Serialize(BaseGraphic graphic)
        {
            return JsonConvert.SerializeObject(graphic);
        }

        public static BaseGraphic Deserialize(string json) // todo: find better way to init graphic
        {
            dynamic? jsonGraphic = JsonConvert.DeserializeObject(json);

            if (jsonGraphic is null)
            {
                throw new NullReferenceException();
            }

            BaseGraphic graphic;
            List<LinearPoint> points = jsonGraphic.LinearPoints.ToObject<List<LinearPoint>>();

            switch ((GraphicType)jsonGraphic.Type)
            {
                case GraphicType.Orthodrome:
                    graphic = new OrthodromeGraphic(points.ToCoordinates().ToList());
                    InitCommonProperties(jsonGraphic, graphic);
                    return graphic;

                case GraphicType.Point:
                    graphic = new PointGraphic(points.ToCoordinates().ToList()) { Image = jsonGraphic.Image.ToObject<string?>() };
                    InitCommonProperties(jsonGraphic, graphic);
                    return graphic;

                case GraphicType.Rectangle:
                    graphic = new RectangleGraphic(points.ToCoordinates().ToList());
                    InitCommonProperties(jsonGraphic, graphic);
                    return graphic;

                case GraphicType.Polygon:
                    graphic = new PolygonGraphic(points.ToCoordinates().ToList());
                    InitCommonProperties(jsonGraphic, graphic);
                    return graphic;

                default:
                    throw new NotImplementedException();
            }
        }

        private static void InitCommonProperties(dynamic jsonGraphic, BaseGraphic graphic)
        {
            graphic.Color = jsonGraphic.Color.ToObject<Color?>();
            graphic.UserTags = jsonGraphic.UserTags.ToObject<Dictionary<string, IUserTag>>();
            graphic.Opacity = jsonGraphic.Opacity.ToObject<double>();
        }
    }
}