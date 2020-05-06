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

        public static (float H, float S, float L) ColorToHsl(Color color) {
            return (color.GetHue(), color.GetSaturation(), color.GetBrightness());
        }

        public static Color HslToColor((float H, float S, float L) color) {
            float chroma = (1-MathF.Abs(2*color.L-1))*color.S;
            float h = color.H/60;
            float x = chroma*(1-MathF.Abs(h%2-1));
            (float r, float g, float b) result = (0, 0, 0);
            if ((int) h == 0) {
                result.r = chroma;
                result.g = x;
            } else if ((int) h == 1) {
                result.r = x;
                result.g = chroma;
            } else if ((int) h == 2) {
                result.g = chroma;
                result.b = x;
            } else if ((int) h == 3) {
                result.g = x;
                result.b = chroma;
            } else if ((int) h == 4) {
                result.r = x;
                result.b = chroma;
            } else if ((int) h == 5) {
                result.r = chroma;
                result.b = x;
            }
            float m = color.L-chroma/2;
            //return Color.Black;
            return Color.FromArgb((int) MathF.Round((result.r+m)*255), (int) MathF.Round((result.g+m)*255), (int) MathF.Round((result.b+m)*255));
        }

        public static Color HslToColor(float H, float S, float L) {
            return HslToColor((H, S, L));
        }

        public static string ColorToHex(Color color) {
            return $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}";
        }

        public static Color HexToColor(string hex) {
            hex = hex.Trim().Replace("#", "").ToLower();
            int r = int.Parse(""+hex[0]+hex[1], System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(""+hex[2]+hex[3], System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(""+hex[4]+hex[5], System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(r, g, b);
        }

        public static Color ClampRgbToColor(int r, int g, int b) {
            return Color.FromArgb(Math.Max(0, Math.Min(255, r)), Math.Max(0, Math.Min(255, g)), Math.Max(0, Math.Min(255, b)));
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
    }
}