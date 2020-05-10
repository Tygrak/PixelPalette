using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace PixelPalette.Algorithm {
    public static class Ditherer {
        public static Bitmap ClosestColor(Bitmap bitmap, Color[] colorPalette) {
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Parallel.For(0, bitmap.Width, x => {
                for (int y = 0; y < height; y++) {
                    Color pixel = colors[x+y*width];
                    result.SetPixel(x, y, ColorHelpers.GetMinDistance(pixel, colorPalette));
                    //result.SetPixel(x, y, pixel);
                }    
            });
            return result.Bitmap;
        }

        private static void AddToPixelInArray((int R, int G, int B)[] colors, int x, int y, int width, (int R, int G, int B) toAdd, float multiplier) {
            int pos = x+y*width;
            if (x >= width || x < 0 || pos < 0 || pos >= colors.Length) {
                return;
            }
            colors[pos] = ((int) (colors[pos].R+toAdd.R*multiplier), (int) (colors[pos].G+toAdd.G*multiplier), (int) (colors[pos].B+toAdd.B*multiplier));
        }

        public static Bitmap FloydSteinbergDither(Bitmap bitmap, Color[] colorPalette, float[] errorMatrix) {
            if (errorMatrix == null || errorMatrix.Length < 4) {
                throw new ArgumentException("Error diffusion matrix needs to have 4 values");
            }
            (int R, int G, int B)[] colors = BitmapConvert.IntArrayFromBitmap(bitmap);
            int width = bitmap.Width;
            int height = bitmap.Height;
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    (int R, int G, int B) pixel = colors[x+y*width];
                    Color nearestColor = ColorHelpers.GetMinDistance(pixel, colorPalette);
                    (int R, int G, int B) error = (pixel.R-nearestColor.R, pixel.G-nearestColor.G, pixel.B-nearestColor.B);
                    AddToPixelInArray(colors, x+1, y  , width, error, errorMatrix[0]);
                    AddToPixelInArray(colors, x-1, y+1, width, error, errorMatrix[1]);
                    AddToPixelInArray(colors, x  , y+1, width, error, errorMatrix[2]);
                    AddToPixelInArray(colors, x+1, y+1, width, error, errorMatrix[3]);
                    result.SetPixel(x, y, nearestColor);
                }
            }
            return result.Bitmap;
        }

        public static Bitmap MinAvgErrDither(Bitmap bitmap, Color[] colorPalette, float[] errorMatrix) {
            if (errorMatrix == null || errorMatrix.Length < 12) {
                throw new ArgumentException("Error diffusion matrix needs to have 12 values");
            }
            (int R, int G, int B)[] colors = BitmapConvert.IntArrayFromBitmap(bitmap);
            int width = bitmap.Width;
            int height = bitmap.Height;
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    (int R, int G, int B) pixel = colors[x+y*width];
                    Color nearestColor = ColorHelpers.GetMinDistance(pixel, colorPalette);
                    (int R, int G, int B) error = (pixel.R-nearestColor.R, pixel.G-nearestColor.G, pixel.B-nearestColor.B);
                    AddToPixelInArray(colors, x+1, y  , width, error, errorMatrix[0]);
                    AddToPixelInArray(colors, x+2, y  , width, error, errorMatrix[1]);
                    AddToPixelInArray(colors, x-2, y+1, width, error, errorMatrix[2]);
                    AddToPixelInArray(colors, x-1, y+1, width, error, errorMatrix[3]);
                    AddToPixelInArray(colors, x  , y+1, width, error, errorMatrix[4]);
                    AddToPixelInArray(colors, x+1, y+1, width, error, errorMatrix[5]);
                    AddToPixelInArray(colors, x+2, y+1, width, error, errorMatrix[6]);
                    AddToPixelInArray(colors, x-2, y+2, width, error, errorMatrix[7]);
                    AddToPixelInArray(colors, x-1, y+2, width, error, errorMatrix[8]);
                    AddToPixelInArray(colors, x  , y+2, width, error, errorMatrix[9]);
                    AddToPixelInArray(colors, x+1, y+2, width, error, errorMatrix[10]);
                    AddToPixelInArray(colors, x+2, y+2, width, error, errorMatrix[11]);
                    result.SetPixel(x, y, nearestColor);
                }    
            }
            return result.Bitmap;
        }

        //todo: configurable size
        public static Bitmap OrderedDither(Bitmap bitmap, Color[] colorPalette, float[,] matrix) {
            (int R, int G, int B)[] colors = BitmapConvert.IntArrayFromBitmap(bitmap);
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Parallel.For(0, bitmap.Width, x => {
                for (int y = 0; y < height; y++) {
                    (int R, int G, int B) pixel = colors[x+y*width];
                    float m = matrix[x%matrix.GetLength(0), y%matrix.GetLength(1)];
                    pixel.R = (int) (pixel.R+255/4f*(m-1/2f));
                    pixel.G = (int) (pixel.G+255/4f*(m-1/2f));
                    pixel.B = (int) (pixel.B+255/4f*(m-1/2f));
                    result.SetPixel(x, y, ColorHelpers.GetMinDistance(pixel, colorPalette));
                }    
            });
            return result.Bitmap;
        }

        public static Bitmap RandomDither(Bitmap bitmap, Color[] colorPalette, float bias = 10) {
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Parallel.For(0, bitmap.Width, x => {
                Random random = new Random();
                for (int y = 0; y < height; y++) {
                    Color pixel = colors[x+y*width];
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
            });
            return result.Bitmap;
        }
    }
}