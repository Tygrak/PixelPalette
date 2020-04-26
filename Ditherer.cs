using System;
using Avalonia;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace PixelPalette {
    public class Ditherer {
        public Bitmap ClosestColor(Bitmap bitmap, Color[] colorPalette) {
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    Color pixel = bitmap.GetPixel(x, y);
                    int minDistance = int.MaxValue;
                    int minDistanceIndex = -1;
                    for (int i = 0; i < colorPalette.Length; i++) {
                        int distance = (int) ColorHelpers.GetDistance(colorPalette[i], pixel);
                        if (distance < minDistance) {
                            minDistance = distance;
                            minDistanceIndex = i;
                        }
                    }
                    result.SetPixel(x, y, colorPalette[minDistanceIndex]);
                }    
            }
            return result.Bitmap;
        }

        public Bitmap SortColors(Bitmap bitmap, Color[] colorPalette) {
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            List<List<Color>> colors = new List<List<Color>>(){BitmapConvert.ColorArrayFromBitmap(bitmap).ToList()};
            while (colors.Count*2 <= 8) {
                List<List<Color>> nextColors = new List<List<Color>>(); 
                foreach (var pixels in colors) {
                    var splitted = ColorHelpers.SplitColors(pixels);
                    nextColors.Add(splitted.Item1);
                    nextColors.Add(splitted.Item2);
                }
                colors = nextColors;
            }
            var resColors = new List<Color>();
            foreach (var pixels in colors)
            {
                resColors.AddRange(pixels);
            }
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    result.SetPixel(x, y, resColors[x+y*bitmap.Width]);
                }    
            }
            return result.Bitmap;
        }

        public Bitmap RandomDither(Bitmap bitmap, Color[] colorPalette, float bias = 5) {
            Random random = new Random();
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    Color pixel = bitmap.GetPixel(x, y);
                    int highestDistance = 0;
                    int lowestDistance = int.MaxValue;
                    float distanceSum = 0;
                    float[] distances = new float[colorPalette.Length];
                    for (int i = 0; i < colorPalette.Length; i++) {
                        int distance = (int) ColorHelpers.GetDistance(colorPalette[i], pixel);
                        distances[i] = distance;
                        if (highestDistance < distance) {
                            highestDistance = distance;
                        }
                        if (lowestDistance > distance) {
                            lowestDistance = distance;
                        }
                    }
                    for (int i = 0; i < colorPalette.Length; i++) {
                        distances[i] = MathF.Pow(lowestDistance+highestDistance-distances[i], bias);
                        distanceSum += distances[i];
                    }
                    float choosen = (float) (random.NextDouble()*distanceSum);
                    float currentRange = 0;
                    for (int i = 0; i < distances.Length; i++) {
                        currentRange += distances[i];
                        if (currentRange > choosen) {
                            result.SetPixel(x, y, colorPalette[i]);
                            break;
                        }
                    }
                }    
            }
            return result.Bitmap;
        }
    }
}