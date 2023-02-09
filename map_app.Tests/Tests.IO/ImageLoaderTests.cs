using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using map_app.Services.IO;
using Mapsui.Styles;

namespace map_app.Tests.TestsIO
{
    [TestFixture]
    public class ImageLoaderTests
    {
        private readonly string existedImagePath = GetPathByName("picture.png");

        [Test]
        public async Task LoadAsync_ShouldReturnMinusOne_WhenFileIsNotExist()
        {
            var notExistedPath = "/is/not/exist.png";
            var bitmapId = await ImageLoader.LoadAsync(notExistedPath);
            Assert.That(bitmapId, Is.EqualTo(-1));
        }

        [Test]
        public async Task LoadAsync_ShouldRegisterBitmap_WhenFileIsExist()
        {
            var bitmapId = await ImageLoader.LoadAsync(existedImagePath);
            var bitmap = BitmapRegistry.Instance.Get(bitmapId);
            Assert.IsNotNull(bitmap);
        }

        [TestCase("picture.png")]
        [TestCase("picture.gif")]
        [TestCase("picture.jpg")]
        [TestCase("picture.ico")]
        [TestCase("picture.webp")]
        public async Task LoadAsync_ShouldRegisterRasterBitmap(string fileName)
        {
            var bitmapId = await ImageLoader.LoadAsync(existedImagePath);
            var bitmap = BitmapRegistry.Instance.Get(bitmapId);
            Assert.IsNotNull(bitmap);
        }

        [Test]
        public async Task LoadAsync_ShouldReturnSameIndex_WhenBitmapRegistered()
        {
            var bitmapId1 = await ImageLoader.LoadAsync(existedImagePath);
            var bitmapId2 = await ImageLoader.LoadAsync(existedImagePath);
            Assert.That(bitmapId1, Is.EqualTo(bitmapId2));
        }

        private static string GetPathByName(string fileName)
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
    }
}