using System.Collections;
using map_app.Models;
using map_app.Models.Extensions;

namespace map_app.Tests;

[TestFixture]
public class BaseGraphicExtentionsTests
{
    [Test]
    public void GetSegments_ShouldReturnEmpty_WhenFeatureHaveOneCoordinate()
    {
        TestDistances(Enumerable.Empty<double>(), new PointGraphic());
    }

    [Test]
    public void GetSegments_ShouldReturnLineDistances_WhenIsOrthodrome()
    {
        var expected = new[] { 157.25, 157.23 };
        var orthodrome = new OrthodromeGraphic(new[] { new GeoPoint(), new GeoPoint(1, 1), new GeoPoint(2, 2) }
                .ToWorldPositions()
                .ToList());
        TestDistances(expected, orthodrome);
    }

    [Test]
    public void GetSegments_ShouldReturnRingDistances_WhenIsPolygon()
    {
        var expected = new[] { 157.25, 157.23, 314.5 };
        var polygon = new PolygonGraphic(new[] { new GeoPoint(), new GeoPoint(1, 1), new GeoPoint(2, 2) }
                .ToWorldPositions()
                .ToList());
        TestDistances(expected, polygon);     
    }


    [Test]
    public void GetSegments_ShouldReturnOneDistance_WhenIsPolygonHaveTwoCoordinates()
    {
        var expected = new[] { 157.25 };
        var polygon = new PolygonGraphic(new[] { new GeoPoint(), new GeoPoint(1, 1) }
                .ToWorldPositions()
                .ToList());
        TestDistances(expected, polygon);     
    }
    
    [Test]
    public void GetSegments_ShouldReturnRingDistances_WhenIsRectangle()
    {
        var expected = new[] { 222.4, 222.4, 222.25, 222.4 };
        var rectangle = new RectangleGraphic(new[] { new GeoPoint(), new GeoPoint(2, 2) }
            .ToWorldPositions()
            .ToList());
        TestDistances(expected, rectangle);
    }

    private void TestDistances(IEnumerable<double> expected, BaseGraphic target)
    {
        var actual = target.GetSegments().Select(s => s.Distance);
        CollectionAssert.AreEqual(expected, actual, new DoubleDeltaComparer(0.05));
    }

    private class DoubleDeltaComparer : IComparer
    {
        private readonly double _delta;

        public DoubleDeltaComparer(double delta)
        {
            _delta = delta;
        }

        public int Compare(object? x, object? y)
        {
            if (x is not double num1 || y is not double num2)
                throw new ArgumentException();

            if (Math.Abs(num1 - num2) < _delta)
                return 0;
            return num1 - num2 < 0 ? -1 : 1;
        }
    }
}