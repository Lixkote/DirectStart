using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace B8TAM
{
    class TileColorFromIcon
    {
        public static string CalculateLeftGradient(ImageSource tileicon)
        {
            // Get the main color of the icon
            Color mainColor = GetMainColor(tileicon);

            // Calculate the three color values
            Color gradientLeftPoint = LightenColor(mainColor, 0.7);

            // Display the calculated colors
            return "#" + gradientLeftPoint.R.ToString("X2") + gradientLeftPoint.G.ToString("X2") + gradientLeftPoint.B.ToString("X2");
        }
        public static string CalculateRightGradient(ImageSource tileicon)
        {
            // Get the main color of the icon
            Color mainColor = GetMainColor(tileicon);

            // Calculate the three color values
            Color gradientRightPoint = LightenColor(mainColor, 0.8);
            return "#" + gradientRightPoint.R.ToString("X2") + gradientRightPoint.G.ToString("X2") + gradientRightPoint.B.ToString("X2");
        }
        public static string CalculateBorder(ImageSource tileicon)
        {
            // Get the main color of the icon
            Color mainColor = GetMainColor(tileicon);
            Color border = LightenColor(mainColor, 1);

            // Display the calculated colors
            return "#" + border.R.ToString("X2") + border.G.ToString("X2") + border.B.ToString("X2");
        }

        private static Color GetMainColor(ImageSource imageSource)
        {
            BitmapImage bitmapImage = (BitmapImage)imageSource;

            // Convert BitmapSource to Bitmap
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(outStream);
                using (Bitmap bitmap = new Bitmap(outStream))
                {
                    // Calculate the average color of the bitmap
                    int totalR = 0, totalG = 0, totalB = 0;
                    int pixelCount = 0;

                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            System.Drawing.Color pixelColor = bitmap.GetPixel(x, y);
                            totalR += pixelColor.R;
                            totalG += pixelColor.G;
                            totalB += pixelColor.B;
                            pixelCount++;
                        }
                    }

                    return System.Drawing.Color.FromArgb(totalR / pixelCount, totalG / pixelCount, totalB / pixelCount);
                }
            }
        }

        private static Color DarkenColor(Color color, double percentage)
        {
            // Darken the color by the specified percentage
            double factor = 1 - percentage;
            return Color.FromArgb((byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
        }

        private static Color LightenColor(Color color, double percentage)
        {
            // Lighten the color by the specified percentage
            double factor = 1 + percentage;
            return Color.FromArgb((byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
        }
    }
}
