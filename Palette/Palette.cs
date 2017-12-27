using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace Palette
{
    public static class Palette
    {
        private static ColorThiefDotNet.ColorThief colorThief = new ColorThiefDotNet.ColorThief();

        public static async Task<List<Color>> GeneratePalette(Uri path)
        {
            if (path == null)
            {
                throw new ArgumentNullException();
            }
            //get the file
            if (path.IsFile)
            {
                var file = await StorageFile.GetFileFromPathAsync(path.LocalPath);
                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    return FromColorThief(await colorThief.GetPalette(decoder, 8, 1));
                }
            }
            else
            {
                RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(path);
                using (IRandomAccessStream stream = await random.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    return FromColorThief(await colorThief.GetPalette(decoder, 8, 1));
                }
            }
        }



        public static List<Color> FromColorThief(List<ColorThiefDotNet.QuantizedColor> list)
        {
            return list.Select(x =>
            {
                var d = x.Color;
                var a = d.A;
                var r = d.R;
                var g = d.G;
                var b = d.B;
                return Color.FromArgb(a, r, g, b);
            }).ToList();
        }
    }
}
