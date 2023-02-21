using map_app.Models;
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
    public void GetDistances_ChangeTestName() // polygon and line differ
    {
        var actual = auxiliaryVM.GetDistances(new PointGraphic());
        Assert.That(actual, Is.Empty);
    }
}