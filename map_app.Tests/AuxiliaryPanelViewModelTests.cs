using System.Collections;
using map_app.Models;
using map_app.Models.Extensions;
using map_app.Services;
using map_app.ViewModels;
using map_app.ViewModels.Controls;
using Mapsui.UI.Avalonia;

namespace map_app.Tests;

[TestFixture]
public class AuxiliaryPanelViewModelTests
{
    private MapControl mapControl = new();
    private MainViewModel mainVM;
    private AuxiliaryPanelViewModel auxiliaryVM;

    [SetUp]
    public void SetUp()
    {
        mapControl.Map = MapCreator.Create();
        var graphics = mapControl.Map.Layers.First(x => x.Tag?.ToString() == "Graphic") as OwnWritableLayer;
        graphics!.Add(new PointGraphic());
        mainVM = new MainViewModel(mapControl);
        auxiliaryVM = new AuxiliaryPanelViewModel(mainVM);
    }

    [Test]
    public void GetDistances_ShouldReturnEmpty_WhenFeatureHaveOneCoordinate()
    {
        var actual = auxiliaryVM.GetDistances(new PointGraphic());
        Assert.That(actual, Is.Empty);
    }

    [Test]
    public void GetDistances_ChangeTestName()
    {
        var actual = auxiliaryVM.GetDistances(new PointGraphic());
        Assert.That(actual, Is.Empty);
    }

    [Test]
    public void GetDistances_ShouldReturnLineDistance_WhenOrthodrome()
    {
        var expected = new[] { 157.25, 157.23 };
        var actual = auxiliaryVM.GetDistances(new OrthodromeGraphic(
            new[] { new GeoPoint(), new GeoPoint(1, 1), new GeoPoint(2, 2) }
                .ToWorldPositions()
                .ToList()));
        CollectionAssert.AreEqual(expected, actual, new DoubleDeltaComparer(1e-2));
    }

    // [Test]
    // public void GetDistances_ShouldReturnLineDistance_WhenOrthodrome()
    // {
    //     var expected = 157.25;
    //     var actual = auxiliaryVM.GetDistances(new OrthodromeGraphic(
    //         new[] { new GeoPoint(), new GeoPoint(1, 1) }
    //             .ToWorldPositions()
    //             .ToList()));
    //     Assert.AreEqual(expected, actual.Single(), 1e-3);
    // }

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