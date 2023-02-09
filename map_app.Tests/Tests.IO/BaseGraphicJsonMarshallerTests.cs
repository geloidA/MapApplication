using System.Collections;
using map_app.Models;
using map_app.Services;
using map_app.Services.IO;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace map_app.Tests.TestsIO
{
    [TestFixture]
    public class BaseGraphicJsonMarshallerTests
    {
        private static readonly List<BaseGraphic> graphics;
        private static readonly string wrongJsonLocation;
        private static readonly string fileName;

        static BaseGraphicJsonMarshallerTests()
        {
            fileName = "/home/user/practics/MapApplication/map_app.Tests/file.txt";
            wrongJsonLocation = "/home/user/practics/help.txt";
            graphics = new List<BaseGraphic>
            {
                new PointGraphic(new[] { new Coordinate3D() }.Cast<Coordinate>().ToList()),
                new RectangleGraphic(new[] { new Coordinate3D(), new Coordinate3D() }.Cast<Coordinate>().ToList())
            };
        }

        [TearDown]
        public void DeleteFile()
        {
            File.Delete(fileName);
        }

        [Test]
        public async Task SaveAsync_ShouldSaveText()
        {
            await SaveFile();
            var info = new FileInfo(fileName);
            FileAssert.Exists(info);
        }

        [Test]
        public async Task SaveAsync_ShouldSaveEmptyFile_WhenGraphicsIsEmpty()
        {
            await BaseGraphicJsonMarshaller.SaveAsync(Array.Empty<BaseGraphic>(), fileName);
            var text = GetTextFromFile(new FileInfo(fileName));
            Assert.IsEmpty(text);
        }

        [Test]
        public async Task SaveAsync_ShouldSaveObjectJsonText()
        {
            await SaveFile();
            var expected = string.Join('\n', GraphicSerializer.Serialize(graphics));
            var actual = GetTextFromFile(new FileInfo(fileName));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task LoadAsync_ShouldLoadObjects()
        {
            await SaveFile();
            var info = new FileInfo(fileName);
            var container = new OwnWritableLayer();
            await BaseGraphicJsonMarshaller.LoadAsync(container, info.FullName);
            CollectionAssert.AreEqual(graphics, container, new BaseGraphicComparer());
        }

        [Test]
        public async Task LoadAsync_ShouldThrowFileLoadException_WhenJsonWrong()
        {
            await SaveFile();
            var info = new FileInfo(wrongJsonLocation);
            var container = new OwnWritableLayer();
            Assert.ThrowsAsync<JsonReaderException>(async () => await BaseGraphicJsonMarshaller.LoadAsync(container, info.FullName));
        }

        private async Task SaveFile()
        {
            await BaseGraphicJsonMarshaller.SaveAsync(graphics, fileName);
        }

        private string GetTextFromFile(FileInfo file)
        {
            var jsons = new List<string>();
            using (var reader = file.OpenText())
            {
                Assert.IsNotNull(reader);
                string? json;
                while ((json = reader!.ReadLine()) != null)
                    jsons.Add(json);
            }
            return string.Join('\n', jsons);
        }

        private class BaseGraphicComparer : IComparer
        {
            private const double Eps = 1e-5;
            public int Compare(object? obj1, object? obj2)
            {
                if (obj1 is not BaseGraphic x ||
                    obj2 is not BaseGraphic y) throw new ArgumentException("Values is not BaseGraphic");
                if (Equals(x, y)) return 0;
                throw new NotImplementedException();
            }

            private bool Equals(BaseGraphic x, BaseGraphic y)
            {
                var linearPointEquals = x.LinearPoints.SequenceEqual(y.LinearPoints, 
                    new ThreeDimentionalPointEqualityComparer());
                var geoPointEquals = x.GeoPoints.SequenceEqual(y.GeoPoints, 
                    new ThreeDimentionalPointEqualityComparer());                
                var tagsEquals = x.UserTags?.Count == y.UserTags?.Count 
                    && DictionaryEquals(x.UserTags, y.UserTags);
                return x.Type == y.Type 
                    && x.StyleColor.Equals(y.StyleColor)
                    && linearPointEquals
                    && geoPointEquals
                    && tagsEquals
                    && Math.Abs(x.Opacity - y.Opacity) < Eps;
            }

            private bool DictionaryEquals(Dictionary<string, string>? x, Dictionary<string, string>? y) 
            {
                if (x is null && y is null) return true;
                if (x is null || y is null) return false;
                return !x.Except(y ?? new Dictionary<string, string>()).Any();
            }
        }
    }
}