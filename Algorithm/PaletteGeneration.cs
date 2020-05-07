using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace PixelPalette.Algorithm {
    public static class PaletteGeneration {
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
                resultColors.Add(ColorHelpers.AverageColors(pixels));
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
                    var splitted = PaletteGeneration.SplitColors(pixels);
                    nextColors.Add(splitted.Item1);
                    nextColors.Add(splitted.Item2);
                }
                colors = nextColors;
            }
            while (colors.Count < count) {
                var splitted = PaletteGeneration.SplitColors(colors[colors.Count-1]);
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

        //initialization could be improved
        public static List<Color> KMeans(Bitmap bitmap, int count, int maxSteps) {
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            int[] clusters = new int[colors.Length];
            Color[] means = new Color[count];
            {
                List<Color> sorted = colors.ToList();
                sorted.Sort((a, b) => a.GetHue().CompareTo(b.GetHue()));
                for (int i = 0; i < count; i++) {
                    means[i] = sorted[i*(sorted.Count/count) + (sorted.Count/count)/2];
                }
            }
            bool changed = false;
            for (int step = 0; step < maxSteps; step++) {
                for (int i = 0; i < colors.Length; i++) {
                    int newCluster = ColorHelpers.GetMinDistanceIndex(colors[i], means);
                    if (newCluster != clusters[i]) {
                        changed = true;
                        clusters[i] = newCluster;
                    }
                }
                if (!changed) {
                    break;
                }
                int[] clusterCounts = new int[count];
                (int R, int G, int B)[] clusterSums = new (int R, int G, int B)[count];
                for (int i = 0; i < colors.Length; i++) {
                    clusterCounts[clusters[i]]++;
                    clusterSums[clusters[i]].R += colors[i].R;
                    clusterSums[clusters[i]].G += colors[i].G;
                    clusterSums[clusters[i]].B += colors[i].B; 
                }
                for (int i = 0; i < count; i++) {
                    means[i] = Color.FromArgb(clusterSums[i].R/clusterCounts[i], clusterSums[i].G/clusterCounts[i], clusterSums[i].B/clusterCounts[i]);
                }
            }
            return means.ToList();
        }
        
        public static List<Color> SectorsHighMidLow(Bitmap bitmap, int count) {
            List<Color> candidates = new List<Color>(count);
            List<Color> colors = BitmapConvert.ColorArrayFromBitmap(bitmap).ToList();
            int sectors = count*3;
            for (int i = 0; i < sectors; i++) {
                List<Color> range = colors.GetRange((colors.Count/sectors)*i, colors.Count/sectors);
                range.Sort((a, b) => a.GetBrightness().CompareTo(b.GetBrightness()));
                candidates.Add(ColorHelpers.AverageColors(range.GetRange(0, range.Count/3)));
                candidates.Add(ColorHelpers.AverageColors(range.GetRange(range.Count/3, range.Count/3)));
                candidates.Add(ColorHelpers.AverageColors(range.GetRange((range.Count*2)/3, range.Count/3)));
            }
            List<Color> result = new List<Color>(count);
            candidates.Sort((a, b) => a.GetBrightness().CompareTo(b.GetBrightness()));
            result.Add(candidates[0]);
            candidates.RemoveAt(0);
            while (result.Count < count) {
                int bestColorId = 0;
                float maxDistance = int.MinValue;
                for (int i = 0; i < candidates.Count; i++) {
                    float minDistance = int.MaxValue;
                    for (int j = 0; j < result.Count; j++) {
                        float distance = ColorHelpers.GetDistance(candidates[i], result[j]);
                        if (distance < minDistance) {
                            minDistance = distance;
                        }
                    }
                    if (minDistance > maxDistance) {
                        bestColorId = i;
                        maxDistance = minDistance;
                    }
                }
                result.Add(candidates[bestColorId]);
                candidates.RemoveAt(bestColorId);
            }
            return result;
        }

        public static List<Color> FakeDuoTone(int count, float hue, float saturation) {
            List<Color> result = new List<Color>();
            for (int i = 0; i < count; i++) {
                result.Add(ColorConvertors.HslToColor((hue, saturation, (float) i/count+1f/(count*2))));
            }
            return result;
        }
    }
}