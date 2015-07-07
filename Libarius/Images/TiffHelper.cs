using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Libarius.Images
{
    public static class TiffHelper
    {
        public static IEnumerable<Image> GetAllPages(string file)
        {
            var bitmap = (Bitmap) Image.FromFile(file);

            return AllPages(bitmap);
        }

        public static IEnumerable<Image> GetAllPages(Stream stream)
        {
            var bitmap = (Bitmap)Image.FromStream(stream);

            return AllPages(bitmap);
        }

        private static IEnumerable<Image> AllPages(Bitmap bitmap)
        {
            var images = new List<Image>();
            var count = bitmap.GetFrameCount(FrameDimension.Page);
            for (var idx = 0; idx < count; idx++)
            {
                // save each frame to a bytestream
                bitmap.SelectActiveFrame(FrameDimension.Page, idx);
                var byteStream = new MemoryStream();
                bitmap.Save(byteStream, ImageFormat.Tiff);

                // and then create a new Image from it
                images.Add(Image.FromStream(byteStream));
            }
            return images;
        }
    }
}