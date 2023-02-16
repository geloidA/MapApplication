using System;
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
        /// <param name="imagePath"></param>
        /// <returns>Return registered by BitmapRegistry bitmapId</returns>
        public static async Task<int?> RegisterAsync(string imagePath)
        {
            if (!File.Exists(imagePath)) return null;
            if (BitmapRegistry.Instance.TryGetBitmapId(imagePath, out int bitmapId))
                return bitmapId;
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
                new FileInfo(imagePath).Name);
            File.Copy(imagePath, destPath, true);
            return destPath;
            // todo: hide sessionresourses from user or show error message
        }
    }
}