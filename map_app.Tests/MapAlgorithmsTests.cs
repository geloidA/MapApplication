using map_app.Models;
using map_app.Services;

namespace map_app.Tests;

[TestFixture]
public class MapAlgorithmsTests
{
    [Test]
    public void Haversine_MoscowYekaterinburg()
    {
        var expected = 1416.2;
        var moscow = new GeoPoint(37.617698, 55.755864);
        var yekaterinburg = new GeoPoint(60.597474, 56.838011);
        var actual = MapAlgorithms.Haversine(moscow, yekaterinburg);
        Assert.AreEqual(expected, actual, 0.3);
    }

    [Test]
    public void Haversine_NewYorkLosAngeles()
    {
        var expected = 3936;
        var newyork = new GeoPoint(-74.0028, 40.714606);
        var losangeles = new GeoPoint(-118.246139, 34.055863);
        var actual = MapAlgorithms.Haversine(newyork, losangeles);
        Assert.AreEqual(expected, actual, 0.3);
    }

    [Test]
    public void Haversine_RabatSueca()
    {
        var expected = 819.2;
        var rabat = new GeoPoint(-6.8326, 34.0133);
        var sueca = new GeoPoint(-0.3111, 39.2026);
        var actual = MapAlgorithms.Haversine(rabat, sueca);
        Assert.AreEqual(expected, actual, 0.3);
    }

    [Test]
    public void Haversine_ShouldThrowNullRefException_WhenPointIsNull()
    {
        Assert.Throws<NullReferenceException>(() => { MapAlgorithms.Haversine(null, new GeoPoint()); });
    }

    [Test]
    public void Haversine_ShouldReturnZero_WhenPointsSame()
    {
        var point = new GeoPoint();
        Assert.That(MapAlgorithms.Haversine(point, point), Is.EqualTo(0.0));
    }

    [Test]
    public void GetOrthodromePath_ShouldHaveEqualStartEnd()
    {
        var geo1 = new GeoPoint(12, 15);
        var geo2 = new GeoPoint(2, 12);
        var path = MapAlgorithms.GetOrthodromePath(geo1, geo2);
        var last = path.Last();
        Assert.That(new ThreeDimentionalPointEqualityComparer().Equals(geo1, path.First()), Is.True);
        Assert.That(new ThreeDimentionalPointEqualityComparer().Equals(geo2, last), Is.True);
    }
}