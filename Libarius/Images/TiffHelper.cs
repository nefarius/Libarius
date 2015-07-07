using System;
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
            var bitmap = (Bitmap) Image.FromStream(stream);

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

        /// <summary>
        ///     <see href="http://www.codeproject.com/Articles/15186/Bitonal-TIFF-Image-Converter-for-NET">Original source.</see>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filename"></param>
        public static void SaveAsTiff(this Bitmap source, string filename)
        {
            // Get an ImageCodecInfo object that represents the TIFF codec.
            var imageCodecInfo = GetEncoderInfo("image/tiff");
            var encoder = Encoder.Compression;
            var encoderParameters = new EncoderParameters(1);

            // Save the bitmap as a TIFF file with group IV compression.
            var encoderParameter = new EncoderParameter(encoder, (long) EncoderValue.CompressionCCITT4);
            encoderParameters.Param[0] = encoderParameter;
            source.Save(filename, imageCodecInfo, encoderParameters);
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}