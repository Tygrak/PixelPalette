using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace PixelPalette.Algorithm {
    public static class ColorHelpers {
        public static float GetDistance(Color a, Color b) {
            return MathF.Sqrt(MathF.Pow(a.R-b.R, 2)+MathF.Pow(a.G-b.G, 2)+MathF.Pow(a.B-b.B, 2));
        }

        public static float GetDistance((int R, int G, int B) a, Color b) {
            return MathF.Sqrt(MathF.Pow(a.R-b.R, 2)+MathF.Pow(a.G-b.G, 2)+MathF.Pow(a.B-b.B, 2));
        }

        public static float GetDistance(Color a, (int R, int G, int B) b) {
            return GetDistance(b, a);
        }

        public static int GetMinDistanceIndex(Color color, Color[] colorPalette) {
            int minDistance = int.MaxValue;
            int minDistanceId = -1;
            for (int i = 0; i < colorPalette.Length; i++) {
                int distance = (int) ColorHelpers.GetDistance(colorPalette[i], color);
                if (distance < minDistance) {
                    minDistance = distance;
                    minDistanceId = i;
                }
            }
            return minDistanceId;
        }

        public static int GetMinDistanceIndex((int R, int G, int B) color, Color[] colorPalette) {
            int minDistance = int.MaxValue;
            int minDistanceId = -1;
            for (int i = 0; i < colorPalette.Length; i++) {
                int distance = (int) ColorHelpers.GetDistance(colorPalette[i], color);
                if (distance < minDistance) {
                    minDistance = distance;
                    minDistanceId = i;
                }
            }
            return minDistanceId;
        }

        public static Color GetMinDistance(Color color, Color[] colorPalette) {
            return colorPalette[GetMinDistanceIndex(color, colorPalette)];
        }

        public static Color GetMinDistance((int R, int G, int B) color, Color[] colorPalette) {
            return colorPalette[GetMinDistanceIndex(color, colorPalette)];
        }

        public static Color AverageColors(Color a, Color b) {
            return Color.FromArgb(((int) a.R+b.R)/2, ((int) a.G+b.G)/2, ((int) a.B+b.B)/2);
        }

        public static Color AverageColors(List<Color> colors) {
            int r = 0;
            int g = 0;
            int b = 0;
            foreach (var color in colors) {
                r += color.R;
                g += color.G;
                b += color.B;
            }
            return Color.FromArgb(r/colors.Count, g/colors.Count, b/colors.Count);
        }

        public static double CalculateAverageError(Bitmap a, Bitmap b) {
            Color[] aColors = BitmapConvert.ColorArrayFromBitmap(a);
            Color[] bColors = BitmapConvert.ColorArrayFromBitmap(b);
            double error = 0;
            for (int i = 0; i < aColors.Length; i++) {
                error += GetDistance(aColors[i], bColors[i]);
            }
            return error/aColors.Length;
        }

        public static double CalculateAverageBrightnessError(Bitmap a, Bitmap b) {
            Color[] aColors = BitmapConvert.ColorArrayFromBitmap(a);
            Color[] bColors = BitmapConvert.ColorArrayFromBitmap(b);
            double error = 0;
            for (int i = 0; i < aColors.Length; i++) {
                error += aColors[i].GetBrightness()-bColors[i].GetBrightness();
            }
            return error/aColors.Length;
        }
    }
}