using System.IO;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace map_app.Services.IO
{
    public static class ImageRegister
    {
        /// <summary>
        /// Load image in internal folder
        /// </summary>
        /// <param name="importedImagePath"></param>
        /// <returns>Return registered by BitmapRegistry bitmapId</returns>
        public static async Task<int?> RegisterAsync(string importedImagePath)
        {
            if (!File.Exists(importedImagePath)) return null;
            if (BitmapRegistry.Instance.TryGetBitmapId(importedImagePath, out int bitmapId))
                return bitmapId;
            using (var fileStream = new FileStream(importedImagePath, FileMode.Open))
            {
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                bitmapId = BitmapRegistry.Instance.Register(memoryStream, importedImagePath);
            }
            return bitmapId;
        }
        
        public static void EmbedImage(string imagePath)
        {
            var imageLocation = Path.Combine(App.ImportImagesLocation, new FileInfo(imagePath).Name);
            if (File.Exists(imageLocation)) return;
            File.Copy(imagePath, imageLocation);
        }
    }
}