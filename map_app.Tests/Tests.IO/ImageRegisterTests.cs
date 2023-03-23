using map_app.Services.IO;
using Mapsui.Styles;

namespace map_app.Tests.TestsIO;

[TestFixture]
public class ImageRegisterTests
{
    private readonly string existedImagePath = GetPathByName("picture.png");

    [Test]
    public async Task RegisterAsync_ShouldReturnNull_WhenFileIsNotExist()
    {
        var notExistedPath = "/is/not/exist.png";
        var bitmapId = await ImageRegister.RegisterAsync(notExistedPath);
        Assert.That(bitmapId, Is.EqualTo(null));
    }

    [Test]
    public async Task RegisterAsync_ShouldRegisterBitmap_WhenFileIsExist()
    {
        var bitmapId = await ImageRegister.RegisterAsync(existedImagePath);
        var bitmap = BitmapRegistry.Instance.Get(bitmapId.Value);
        Assert.IsNotNull(bitmap);
    }

    [TestCase("picture.png")]
    [TestCase("picture.gif")]
    [TestCase("picture.jpg")]
    [TestCase("picture.ico")]
    [TestCase("picture.webp")]
    public async Task RegisterAsync_ShouldRegisterRasterBitmap(string fileName)
    {
        var bitmapId = await ImageRegister.RegisterAsync(existedImagePath);
        var bitmap = BitmapRegistry.Instance.Get(bitmapId.Value);
        Assert.IsNotNull(bitmap);
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnSameIndex_WhenBitmapRegistered()
    {
        var bitmapId1 = await ImageRegister.RegisterAsync(existedImagePath);
        var bitmapId2 = await ImageRegister.RegisterAsync(existedImagePath);
        Assert.That(bitmapId1, Is.EqualTo(bitmapId2));
    }

    private static string GetPathByName(string fileName)
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
}