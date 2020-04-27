using System;
using Avalonia;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace PixelPalette {
    public static class Ditherer {
        public static Bitmap ClosestColor(Bitmap bitmap, Color[] colorPalette) {
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    Color pixel = colors[x+y*bitmap.Width];
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

        private static void AddToPixelInArray((int R, int G, int B)[] colors, int x, int y, int width, (int R, int G, int B) toAdd, float multiplier) {
            int pos = x+y*width;
            if (x >= width || x < 0 || pos < 0 || pos >= colors.Length) {
                return;
            }
            colors[pos] = ((int) (colors[pos].R+toAdd.R*multiplier), (int) (colors[pos].G+toAdd.G*multiplier), (int) (colors[pos].B+toAdd.B*multiplier));
        }

        public static Bitmap FloydSteinberg(Bitmap bitmap, Color[] colorPalette) {
            (int R, int G, int B)[] colors = BitmapConvert.IntArrayFromBitmap(bitmap);
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    (int R, int G, int B) pixel = colors[x+y*bitmap.Width];
                    int minDistance = int.MaxValue;
                    int minDistanceId = -1;
                    for (int i = 0; i < colorPalette.Length; i++) {
                        int distance = (int) ColorHelpers.GetDistance(colorPalette[i], pixel);
                        if (distance < minDistance) {
                            minDistance = distance;
                            minDistanceId = i;
                        }
                    }
                    (int R, int G, int B) error = (pixel.R-colorPalette[minDistanceId].R, pixel.G-colorPalette[minDistanceId].G, pixel.B-colorPalette[minDistanceId].B);
                    AddToPixelInArray(colors, x+1, y  , bitmap.Width, error, 7/16f);
                    AddToPixelInArray(colors, x-1, y+1, bitmap.Width, error, 3/16f);
                    AddToPixelInArray(colors, x  , y+1, bitmap.Width, error, 5/16f);
                    AddToPixelInArray(colors, x+1, y+1, bitmap.Width, error, 1/16f);
                    result.SetPixel(x, y, colorPalette[minDistanceId]);
                }    
            }
            return result.Bitmap;
        }

        public static Bitmap RandomDither(Bitmap bitmap, Color[] colorPalette, float bias = 10) {
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            Random random = new Random();
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    Color pixel = colors[x+y*bitmap.Width];
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
        

        /*
        def dither2x2(image, colors, errorR1=1/4, errorR2=1/4):
            pixels = list(image.getdata())
            for i in range(len(pixels)):
                bestColorId = 0
                bestColorDistance = getDistance(pixels[i], colors[0])
                for j in range(1, len(colors)):
                    dist = getDistance(pixels[i], colors[j])
                    if dist < bestColorDistance:
                        bestColorDistance = dist
                        bestColorId = j
                error = numpy.subtract(pixels[i], colors[bestColorId])
                pixels[i] = colors[bestColorId]
                if i%image.width != image.width-1:
                    pixels[i+1] = tuple(numpy.round(numpy.add(pixels[i+1], error*errorR1)).astype(int))
                    if i+image.width+1 < len(pixels):
                        pixels[i+image.width+1] = tuple(numpy.round(numpy.add(pixels[i+image.width+1], error*errorR2).astype(int)))
                if i+image.width < len(pixels):
                    pixels[i+image.width] = tuple(numpy.round(numpy.add(pixels[i+image.width], error*errorR1).astype(int)))
            return pixels
        */
    }
}