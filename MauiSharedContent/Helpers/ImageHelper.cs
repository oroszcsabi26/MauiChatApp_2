using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSharedContent.Helpers
{
    public class ImageHelper
    {
        public static ImageSource ConvertBase64ToImageSource(string p_base64String)
        {
            if (string.IsNullOrWhiteSpace(p_base64String))
            {
                throw new ArgumentException("Invalid Base64 string.", nameof(p_base64String));
            }

            byte[] imageBytes = Convert.FromBase64String(p_base64String);
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }

        public static async Task<string> ConvertImageSourceToBase64(ImageSource p_imageSource)
        {
            if (p_imageSource == null)
            {
                throw new ArgumentNullException(nameof(p_imageSource));
            }

            StreamImageSource streamImageSource = (StreamImageSource)p_imageSource;
            Stream stream = await streamImageSource.Stream(CancellationToken.None);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
    }
}
