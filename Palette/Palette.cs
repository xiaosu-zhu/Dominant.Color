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
        public static Color ColorThiefToColor(this ColorThiefDotNet.Color c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static string GetHexString(this Color c)
        {
            return $"#{c.R.ToString("X2")}{c.G.ToString("X2")}{c.B.ToString("X2")}";
        }

        private static ColorThiefDotNet.ColorThief colorThief = new ColorThiefDotNet.ColorThief();

        public static async Task<List<ColorThiefDotNet.QuantizedColor>> GeneratePalette(Uri path)
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
                    return await colorThief.GetPalette(decoder, 8, 1, false);
                }
            }
            else
            {
                try
                {
                    RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(path);
                    using (IRandomAccessStream stream = await random.OpenReadAsync())
                    {
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        return await colorThief.GetPalette(decoder, 8, 1, false);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static byte RGBtoL(Color color)
        {
            return (byte)(((color.R * 299) + (color.G * 587) + (color.B * 114)) / 1000);
        }

        public static bool IsDarkColor(Color c)
        {
            return (5 * c.G + 2 * c.R + c.B) <= 8 * 128;
        }
    }
}
