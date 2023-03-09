using System;
using System.Linq;
using map_app.Models;
using map_app.Models.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace map_app.Services.IO;

public class BaseGraphicConverter : JsonCreationConverter<BaseGraphic>
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    protected override BaseGraphic Create(Type objectType, JObject jObject)
    {
        var points = jObject.Value<JArray>("LinearPoints")?
            .Select(x => x.ToObject<LinearPoint>());
        if (points is null) throw new Exception();
        return (GraphicType)jObject["Type"]?.Value<int>() switch
        {
            GraphicType.Orthodrome => new OrthodromeGraphic(points!.ToCoordinates().ToList()),
            GraphicType.Point => new PointGraphic(points!.ToCoordinates().ToList()),
            GraphicType.Polygon => new PolygonGraphic(points!.ToCoordinates().ToList()),
            GraphicType.Rectangle => new RectangleGraphic(points!.ToCoordinates().ToList()),
            _ => throw new Exception()
        };
    }
}