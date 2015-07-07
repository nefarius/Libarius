using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Libarius.GDI
{
    /// <summary>
    ///     <see href="http://www.codeproject.com/Articles/15186/Bitonal-TIFF-Image-Converter-for-NET" />
    /// </summary>
    public static class BitonalConverter
    {
        /// <summary>
        ///     Performs a basic color/greyscale to 1-bit per pixel (monochrome) conversion.
        /// </summary>
        /// <param name="source">The original bitmap.</param>
        /// <returns>The converted monochrome bitmap.</returns>
        public static Bitmap ConvertToMono2(Bitmap source)
        {
            return source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format1bppIndexed);
        }

        public static Bitmap ConvertToMonochrome(Bitmap source)
        {
            using (var gr = Graphics.FromImage(source)) // SourceImage is a Bitmap object
            {
                var gray_matrix = new[]
                {
                    new[] {0.299f, 0.299f, 0.299f, 0, 0},
                    new[] {0.587f, 0.587f, 0.587f, 0, 0},
                    new[] {0.114f, 0.114f, 0.114f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                };

                var ia = new ImageAttributes();
                ia.SetColorMatrix(new ColorMatrix(gray_matrix));
                ia.SetThreshold((float) 0.8); // Change this threshold as needed
                var rc = new Rectangle(0, 0, source.Width, source.Height);
                gr.DrawImage(source, rc, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);
            }

            return source;
        }

        /// <summary>
        ///     <see href="http://stackoverflow.com/questions/11472597/converting-a-bitmap-to-monochrome" />
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo1Bit(Bitmap input)
        {
            var masks = new byte[] {0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01};
            var output = new Bitmap(input.Width, input.Height, PixelFormat.Format1bppIndexed);
            var data = new sbyte[input.Width, input.Height];
            var inputData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                var scanLine = inputData.Scan0;
                var line = new byte[inputData.Stride];
                for (var y = 0; y < inputData.Height; y++, scanLine += inputData.Stride)
                {
                    Marshal.Copy(scanLine, line, 0, line.Length);
                    for (var x = 0; x < input.Width; x++)
                    {
                        data[x, y] = (sbyte) (64*(GetGreyLevel(line[x*3 + 2], line[x*3 + 1], line[x*3 + 0]) - 0.5));
                    }
                }
            }
            finally
            {
                input.UnlockBits(inputData);
            }
            var outputData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly,
                PixelFormat.Format1bppIndexed);
            try
            {
                var scanLine = outputData.Scan0;
                for (var y = 0; y < outputData.Height; y++, scanLine += outputData.Stride)
                {
                    var line = new byte[outputData.Stride];
                    for (var x = 0; x < input.Width; x++)
                    {
                        var j = data[x, y] > 0;
                        if (j) line[x/8] |= masks[x%8];
                        var error = (sbyte) (data[x, y] - (j ? 32 : -32));
                        if (x < input.Width - 1) data[x + 1, y] += (sbyte) (7*error/16);
                        if (y < input.Height - 1)
                        {
                            if (x > 0) data[x - 1, y + 1] += (sbyte) (3*error/16);
                            data[x, y + 1] += (sbyte) (5*error/16);
                            if (x < input.Width - 1) data[x + 1, y + 1] += (sbyte) (1*error/16);
                        }
                    }
                    Marshal.Copy(line, 0, scanLine, outputData.Stride);
                }
            }
            finally
            {
                output.UnlockBits(outputData);
            }
            return output;
        }

        public static double GetGreyLevel(byte r, byte g, byte b)
        {
            return (r*0.299 + g*0.587 + b*0.114)/255;
        }

        public static Bitmap ConvertToRGB(Bitmap original)
        {
            var newImage = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
            newImage.SetResolution(original.HorizontalResolution, original.VerticalResolution);
            using (var g = Graphics.FromImage(newImage))
            {
                g.DrawImageUnscaled(original, 0, 0);
            }
            return newImage;
        }

        public static Bitmap ConvertToBitonal(Bitmap original)
        {
            Bitmap source = null;

            // If original bitmap is not already in 32 BPP, ARGB format, then convert
            if (original.PixelFormat != PixelFormat.Format32bppArgb)
            {
                source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                using (var g = Graphics.FromImage(source))
                {
                    g.DrawImageUnscaled(original, 0, 0);
                }
            }
            else
            {
                source = original;
            }

            // Lock source bitmap in memory
            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            // Copy image data to binary array
            var imageSize = sourceData.Stride*sourceData.Height;
            var sourceBuffer = new byte[imageSize];
            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

            // Unlock source bitmap
            source.UnlockBits(sourceData);

            // Create destination bitmap
            var destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);
            destination.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            // Lock destination bitmap in memory
            var destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            // Create destination buffer
            imageSize = destinationData.Stride*destinationData.Height;
            var destinationBuffer = new byte[imageSize];

            var sourceIndex = 0;
            var destinationIndex = 0;
            var pixelTotal = 0;
            byte destinationValue = 0;
            var pixelValue = 128;
            var height = source.Height;
            var width = source.Width;
            var threshold = 500;

            // Iterate lines
            for (var y = 0; y < height; y++)
            {
                sourceIndex = y*sourceData.Stride;
                destinationIndex = y*destinationData.Stride;
                destinationValue = 0;
                pixelValue = 128;

                // Iterate pixels
                for (var x = 0; x < width; x++)
                {
                    // Compute pixel brightness (i.e. total of Red, Green, and Blue values) - Thanks murx
                    //                           B                             G                              R
                    pixelTotal = sourceBuffer[sourceIndex] + sourceBuffer[sourceIndex + 1] +
                                 sourceBuffer[sourceIndex + 2];
                    if (pixelTotal > threshold)
                    {
                        destinationValue += (byte) pixelValue;
                    }
                    if (pixelValue == 1)
                    {
                        destinationBuffer[destinationIndex] = destinationValue;
                        destinationIndex++;
                        destinationValue = 0;
                        pixelValue = 128;
                    }
                    else
                    {
                        pixelValue >>= 1;
                    }
                    sourceIndex += 4;
                }
                if (pixelValue != 128)
                {
                    destinationBuffer[destinationIndex] = destinationValue;
                }
            }

            // Copy binary image data to destination bitmap
            Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

            // Unlock destination bitmap
            destination.UnlockBits(destinationData);

            // Dispose of source if not originally supplied bitmap
            if (source != original)
            {
                source.Dispose();
            }

            // Return
            return destination;
        }
    }
}