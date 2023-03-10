using map_app.Models;
using map_app.Services;
using map_app.Services.IO;
using map_app.Tests.Utils;
using NetTopologySuite.Geometries;

namespace map_app.Tests.TestsIO
{
    [TestFixture]
    public class MapStateJsonMarshallerTests
    {
        private static readonly List<BaseGraphic> graphics;
        private static readonly string wrongJsonPath = "../net7.0/Resources/wrongJson.json";
        private static readonly string correctJsonPath = "../net7.0/Resources/correctJson.json";
        private static readonly string saveJsonLocation = "../net7.0/Resources/savedJson.json";

        static MapStateJsonMarshallerTests()
        {
            graphics = new List<BaseGraphic>
            {
                new PointGraphic(new[] { new Coordinate3D() }.Cast<Coordinate>().ToList()),
                new RectangleGraphic(new[] { new Coordinate3D(), new Coordinate3D() }.Cast<Coordinate>().ToList())
            };
        }

        [TearDown]
        public void DeleteSavedFile()
        {
            File.Delete(saveJsonLocation);
        }

        [Test]
        public async Task SaveAsync_ShouldCreateFile()
        {
            await SaveFile(graphics);
            FileAssert.Exists(new FileInfo(saveJsonLocation));
        }

        [Test]
        public async Task SaveAsync_ShouldSave_WhenGraphicsIsEmpty()
        {
            await SaveFile(Enumerable.Empty<BaseGraphic>());
            var state = await MapStateJsonMarshaller.LoadAsync(saveJsonLocation);
            Assert.That(state?.Graphics, Is.Empty);
        }

        [Test]
        public async Task LoadAsync_ShouldReturnState_WhenLoadSuccess()
        {
            await SaveFile(graphics);
            var actual = await MapStateJsonMarshaller.LoadAsync(saveJsonLocation);
            Assert.That(actual, Is.Not.Null);
        }

        [Test]
        public async Task Marshaller_ShouldLoadCorrectGraphicSequence_FromSaved()
        {
            await SaveFile(graphics);
            var state = await MapStateJsonMarshaller.LoadAsync(saveJsonLocation);
            CollectionAssert.AreEqual(graphics, state?.Graphics, new BaseGraphicComparer());
        }

        [Test]
        public async Task LoadAsync_ShouldReturnNull_WhenJsonWrong()
        {
            await SaveFile(graphics);
            var actual = await MapStateJsonMarshaller.LoadAsync(wrongJsonPath);
            Assert.That(actual, Is.Null);
        }

        private async Task SaveFile(IEnumerable<BaseGraphic> values)
        {
            await MapStateJsonMarshaller.SaveAsync(new MapState { Graphics = values }, saveJsonLocation);
        }      
    }
}