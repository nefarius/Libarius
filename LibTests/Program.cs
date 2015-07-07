using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Libarius.Active_Directory;
using Libarius.GDI;
using Libarius.Images;
using Libarius.Network;
using Libarius.System;

namespace LibTests
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var frame in TiffHelper.GetAllPages("TEST.tif"))
            {
                var image = new Bitmap(frame);

                var rgbBitmap = BitonalConverter.ConvertToRGB(image);

                //var bitonalBitmap = BitionalConverter.ConvertToMonochrome(rgbBitmap);

                //bitonalBitmap.SaveAsTiff(string.Format("{0}.tif", Guid.NewGuid()));

                BitonalConverter.ConvertTo1Bit(rgbBitmap).SaveAsTiff("ConvertTo1Bit.tif");

                BitonalConverter.ConvertToBitonal(rgbBitmap).SaveAsTiff("ConvertToBitonal.tif");

                BitonalConverter.ConvertToMono2(rgbBitmap).SaveAsTiff("ConvertToMono2.tif");

                BitonalConverter.ConvertToMonochrome(rgbBitmap).SaveAsTiff("ConvertToMonochrome.tif");

                return;
            }
        }
    }
}
