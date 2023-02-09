using System;
using System.IO;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace map_app.Services.IO
{
    public static class ImageLoader
    {
        /// <summary>
        /// Load image in internal folder
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns>Return registered by BitmapRegistry bitmapId</returns>
        internal static async Task<int> LoadAsync(string imagePath)
        {
            if (imagePath == null) throw new ArgumentNullException("imagePath can't be null");
            int bitmapId;
            using (var fileStream = new FileStream(EmbedImage(imagePath), FileMode.Open))
            {
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                bitmapId = BitmapRegistry.Instance.Register(memoryStream, imagePath);
            }
            return bitmapId;
        }
        
        private static string EmbedImage(string imagePath)
        {
            var destPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "SessionUserImages",
                new FileInfo(imagePath).Name);// todo: equals assertion
            File.Copy(imagePath, destPath, true);
            return destPath;
        }
    }
}