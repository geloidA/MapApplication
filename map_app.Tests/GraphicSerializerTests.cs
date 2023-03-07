using map_app.Models;
using map_app.Services;
using map_app.Tests.Utils;
using NetTopologySuite.Geometries;

namespace map_app.Tests;

public class GraphicSerializerTests
{
    static object[] Cases = new[]
    {
        new object[] 
        { 
            new OrthodromeGraphic(new[] { new Coordinate3D(), new Coordinate3D(0, 1, 1) }.Cast<Coordinate>().ToList())
            { 
                UserTags = new Dictionary<string, string>(),
                StyleColor = Mapsui.Styles.Color.Black,
                Opacity = 0
            }, 
            "{\"Type\":2,\"Name\":null,\"UserTags\":{},\"Color\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"Opacity\":0.0,\"GeoPoints\":[{\"Lon\":0.0,\"Lat\":0.0,\"Alt\":0.0},{\"Lon\":0.0,\"Lat\":8.983152840993817E-06,\"Alt\":1.0}],\"LinearPoints\":[{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},{\"X\":0.0,\"Y\":1.0,\"Z\":1.0}]}"
        },
        new object[] 
        { 
            new PointGraphic(new[] { new Coordinate3D() }.Cast<Coordinate>().ToList())
            { 
                UserTags = new Dictionary<string, string>(), 
                StyleColor = Mapsui.Styles.Color.Black, 
                Scale = 0.5,
                Opacity = 0
            }, 
            "{\"Image\":null,\"Scale\":0.5,\"Type\":0,\"Name\":null,\"UserTags\":{},\"Color\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"Opacity\":0.0,\"GeoPoints\":[{\"Lon\":0.0,\"Lat\":0.0,\"Alt\":0.0}],\"LinearPoints\":[{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}]}"
        },
        new object[] 
        { 
            new PolygonGraphic(new[] { new Coordinate3D(), new Coordinate3D(), new Coordinate3D() }.Cast<Coordinate>().ToList())
            { 
                UserTags = new Dictionary<string, string>(), 
                StyleColor = Mapsui.Styles.Color.Black, 
                Opacity = 0
            }, 
            "{\"Type\":3,\"Name\":null,\"UserTags\":{},\"Color\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"Opacity\":0.0,\"GeoPoints\":[{\"Lon\":0.0,\"Lat\":0.0,\"Alt\":0.0},{\"Lon\":0.0,\"Lat\":0.0,\"Alt\":0.0},{\"Lon\":0.0,\"Lat\":0.0,\"Alt\":0.0}],\"LinearPoints\":[{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}]}"
        },
        new object[] 
        { 
            new RectangleGraphic(new[] { new Coordinate3D(), new Coordinate3D() }.Cast<Coordinate>().ToList())
            { 
                UserTags = new Dictionary<string, string>
                {
                    {"a", "1"},
                    {"b", "[1,2,3]"},
                    {"c", "string"},
                    {"d", "[1.2,2.3,3.4]"}
                }, 
                StyleColor = Mapsui.Styles.Color.Black, 
                Opacity = 0
            }, 
            "{\"Type\":1,\"Name\":null,\"UserTags\":{\"a\":\"1\",\"b\":\"[1,2,3]\",\"c\":\"string\",\"d\":\"[1.2,2.3,3.4]\"},\"Color\":{\"R\":0,\"G\":0,\"B\":0,\"A\":255},\"Opacity\":0.0,\"GeoPoints\":[{\"Lon\":0.0,\"Lat\":0.0,\"Alt\":0.0},{\"Lon\":0.0,\"Lat\":0.0,\"Alt\":0.0}],\"LinearPoints\":[{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}]}"
        }
    };


    [TestCaseSource(nameof(Cases))]
    public void SerializeTests(BaseGraphic graphic, string expected)
    {
        var actual = GraphicSerializer.Serialize(graphic);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(Cases))]
    public void DeserializeTests(BaseGraphic expected, string json)
    {
        var actual = GraphicSerializer.Deserialize(json);
        Assert.That(actual.GetType(), Is.EqualTo(expected.GetType()));
        Assert.That(actual.Opacity, Is.EqualTo(expected.Opacity));
        Assert.That(actual.StyleColor, Is.EqualTo(expected.StyleColor));
        CollectionAssert.AreEqual(expected.UserTags, actual.UserTags);
        CollectionAssert.AreEqual(expected.LinearPoints, actual.LinearPoints, new ThreeDimensionalPointComparer());
        CollectionAssert.AreEqual(expected.GeoPoints, actual.GeoPoints, new ThreeDimensionalPointComparer());
        if (actual is PointGraphic pointActual)
        {
            var pointExpected = (PointGraphic)expected;
            Assert.That(pointActual.Image, Is.EqualTo(pointExpected.Image));
        }
    }
}