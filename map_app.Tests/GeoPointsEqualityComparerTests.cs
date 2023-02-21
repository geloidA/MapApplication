using map_app.Models;
using map_app.Services;

namespace map_app.Tests;

[TestFixture]
public class GeoPointsEqualityComparerTests
{
    private readonly ThreeDimentionalPointEqualityComparer comparer = new();
    
    [Test]
    public void Equals_ShouldReturnTrue_WhenPointsAlmostSame()
    {
        var first = new GeoPoint(0.000001, 0.000001, 0.000001);
        var second = new GeoPoint();
        Assert.IsTrue(comparer.Equals(first, second));
    }

    [Test]
    public void Equals_ShouldReturnFalse_WhenPointsDifferences()
    {
        var first = new GeoPoint(10, 1, 5);
        var second = new GeoPoint();
        Assert.IsFalse(comparer.Equals(first, second));
    }

    [Test]
    public void Equals_ShouldReturnTrue_WhenPointsSameObject()
    {
        var first = new GeoPoint(10, 1, 5);
        Assert.IsTrue(comparer.Equals(first, first));
    }
}