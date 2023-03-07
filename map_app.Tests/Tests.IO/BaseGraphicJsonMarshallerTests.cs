using map_app.Models;
using map_app.Services;
using map_app.Services.IO;
using map_app.Tests.Utils;
using NetTopologySuite.Geometries;

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
        public async Task SaveAsync_ShouldSaveEmptyFile_WhenGraphicsIsEmpty() // delete?
        {
            await BaseGraphicJsonMarshaller.SaveAsync(new MapState(), fileName);
            var text = GetTextFromFile(new FileInfo(fileName));
            Assert.IsEmpty(text);
        }

        [Test]
        public async Task SaveAsync_ShouldSaveObjectJsonText()
        {
            await SaveFile();
            var expected = string.Join('\n', GraphicSerializer.Serialize(graphics));
            var actual = GetTextFromFile(new FileInfo(fileName));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public async Task LoadAsync_ShouldReturnTrue_WhenLoadSuccess()
        {
            await SaveFile();
            var info = new FileInfo(fileName);
            var container = new List<BaseGraphic>();
            var actual = await BaseGraphicJsonMarshaller.TryLoadAsync(container, info.FullName);
            Assert.That(actual, Is.EqualTo(true));
        }

        [Test]
        public async Task LoadAsync_ShouldLoadObjects()
        {
            await SaveFile();
            var info = new FileInfo(fileName);
            var container = new List<BaseGraphic>();
            await BaseGraphicJsonMarshaller.TryLoadAsync(container, info.FullName);
            CollectionAssert.AreEqual(graphics, container, new BaseGraphicComparer());
        }

        [Test]
        public async Task LoadAsync_ShouldReturnFalse_WhenJsonWrong()
        {
            await SaveFile();
            var info = new FileInfo(wrongJsonLocation);
            var container = new List<BaseGraphic>();
            var actual = await BaseGraphicJsonMarshaller.TryLoadAsync(container, info.FullName);
            Assert.That(actual, Is.EqualTo(false));
        }

        private async Task SaveFile()
        {
            await BaseGraphicJsonMarshaller.SaveAsync(new MapState { Graphics = graphics }, fileName);
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
    }
}