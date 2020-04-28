using System;
using Avalonia;
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

        public static Color GetMinDistance(Color color, Color[] colorPalette) {
            int minDistance = int.MaxValue;
            int minDistanceId = -1;
            for (int i = 0; i < colorPalette.Length; i++) {
                int distance = (int) ColorHelpers.GetDistance(colorPalette[i], color);
                if (distance < minDistance) {
                    minDistance = distance;
                    minDistanceId = i;
                }
            }
            return colorPalette[minDistanceId];
        }

        public static Color GetMinDistance((int R, int G, int B) color, Color[] colorPalette) {
            int minDistance = int.MaxValue;
            int minDistanceId = -1;
            for (int i = 0; i < colorPalette.Length; i++) {
                int distance = (int) ColorHelpers.GetDistance(colorPalette[i], color);
                if (distance < minDistance) {
                    minDistance = distance;
                    minDistanceId = i;
                }
            }
            return colorPalette[minDistanceId];
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

        public static (List<Color>, List<Color>) SplitColors(List<Color> colors) {
            byte[] minColorChannels = new byte[]{255, 255, 255};
            byte[] maxColorChannels = new byte[]{0, 0, 0};
            for (int i = 0; i < colors.Count; i++) {
                if (colors[i].R > maxColorChannels[0]) {
                    maxColorChannels[0] = colors[i].R;
                }
                if (colors[i].G > maxColorChannels[1]) {
                    maxColorChannels[1] = colors[i].G;
                }
                if (colors[i].B > maxColorChannels[2]) {
                    maxColorChannels[2] = colors[i].B;
                }
                if (colors[i].R < minColorChannels[0]) {
                    minColorChannels[0] = colors[i].R;
                }
                if (colors[i].G < minColorChannels[1]) {
                    minColorChannels[1] = colors[i].G;
                }
                if (colors[i].B < minColorChannels[2]) {
                    minColorChannels[2] = colors[i].B;
                }
            }
            int sortUsing = 0;
            for (int i = 1; i < 3; i++) {
                if (maxColorChannels[i]-minColorChannels[i] > maxColorChannels[sortUsing]-minColorChannels[sortUsing]) {
                    sortUsing = i;
                }
            }
            if (sortUsing == 0) {
                colors.Sort((a, b) => a.R.CompareTo(b.R));
            } else if (sortUsing == 1) {
                colors.Sort((a, b) => a.G.CompareTo(b.G));
            } else if (sortUsing == 2) {
                colors.Sort((a, b) => a.B.CompareTo(b.B));
            }
            return (colors.GetRange(0, colors.Count/2), colors.GetRange(colors.Count/2, colors.Count/2+colors.Count%2));
        }

        //todo: not really working
        public static List<Color> MedianCut(Bitmap bitmap, int count) {
            List<List<Color>> colors = new List<List<Color>>(){BitmapConvert.ColorArrayFromBitmap(bitmap).ToList()};
            while (colors.Count*2 <= count) {
                List<List<Color>> nextColors = new List<List<Color>>(); 
                foreach (var pixels in colors) {
                    var splitted = SplitColors(pixels);
                    nextColors.Add(splitted.Item1);
                    nextColors.Add(splitted.Item2);
                }
                colors = nextColors;
            }
            while (colors.Count < count) {
                var splitted = SplitColors(colors[colors.Count-1]);
                colors.RemoveAt(colors.Count-1);
                colors.Add(splitted.Item1);
                colors.Add(splitted.Item2);
            }
            List<Color> resultColors = new List<Color>();
            foreach (var pixels in colors) {
                resultColors.Add(AverageColors(pixels));
            }
            return resultColors;
            //return colors.Select(l => AverageColors(l)).ToList();
        }

        public static Bitmap SortColors(Bitmap bitmap, int count) {
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            List<List<Color>> colors = new List<List<Color>>(){BitmapConvert.ColorArrayFromBitmap(bitmap).ToList()};
            while (colors.Count*2 <= count) {
                List<List<Color>> nextColors = new List<List<Color>>(); 
                foreach (var pixels in colors) {
                    var splitted = ColorHelpers.SplitColors(pixels);
                    nextColors.Add(splitted.Item1);
                    nextColors.Add(splitted.Item2);
                }
                colors = nextColors;
            }
            while (colors.Count < count) {
                var splitted = ColorHelpers.SplitColors(colors[colors.Count-1]);
                colors.RemoveAt(colors.Count-1);
                colors.Add(splitted.Item1);
                colors.Add(splitted.Item2);
            }
            var resColors = new List<Color>();
            foreach (var pixels in colors)
            {
                resColors.AddRange(pixels);
            }
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    if (y > 0 && y%(bitmap.Height/count) == 0) {
                        result.SetPixel(x, y, Color.Black);
                    } else {
                        result.SetPixel(x, y, resColors[x+y*bitmap.Width]);
                    }
                }    
            }
            return result.Bitmap;
        }
    }
}