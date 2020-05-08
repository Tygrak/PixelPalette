using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PixelPalette.Algorithm {
    public static class ColorAdjustments {
        public static Bitmap ShiftHsl(Bitmap bitmap, float hueShift, float saturation, float lightness) {
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    var hsl = ColorConvertors.ColorToHsl(colors[x+y*bitmap.Width]);
                    hsl.H = MathF.Max(hsl.H+hueShift, 0)%360;
                    hsl.S = MathF.Min(MathF.Max((hsl.S)*(1+saturation), 0), 1);
                    hsl.L = MathF.Min(MathF.Max((hsl.L)*(1+lightness), 0), 1);
                    result.SetPixel(x, y, ColorConvertors.HslToColor(hsl));
                }    
            }
            return result.Bitmap;
        }

        public static Bitmap BlurImage(Bitmap bitmap, float sigma) {
            int range = (int) (1+sigma*2f);
            float c = 1/MathF.Sqrt(sigma*sigma*2*MathF.PI);
            float[] blurMatrix = new float[range];
            for (int i = 0; i < range; i++) {
                blurMatrix[i] = MathF.Pow(MathF.E, -(i*i)/(2*sigma*sigma))*c;
            }
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            Color[] colors2 = new Color[colors.Length];
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            Parallel.For(0, bitmap.Width, x => {
                for (int y = 0; y < bitmap.Height; y++) {
                    float r = 0;
                    float g = 0;
                    float b = 0;
                    for (int i = x-range+1; i < x+range; i++) {
                        if (i < 0 || i >= bitmap.Width) {
                            r += colors[x+y*bitmap.Width].R*blurMatrix[Math.Abs(x-i)];
                            g += colors[x+y*bitmap.Width].G*blurMatrix[Math.Abs(x-i)];
                            b += colors[x+y*bitmap.Width].B*blurMatrix[Math.Abs(x-i)];
                        } else {
                            r += colors[i+y*bitmap.Width].R*blurMatrix[Math.Abs(x-i)];
                            g += colors[i+y*bitmap.Width].G*blurMatrix[Math.Abs(x-i)];
                            b += colors[i+y*bitmap.Width].B*blurMatrix[Math.Abs(x-i)];
                        }
                    }
                    colors2[x+y*bitmap.Width] = ColorConvertors.ClampRgbToColor((int) r, (int) g, (int) b);
                }    
            });
            Parallel.For(0, bitmap.Height, y => {
                for (int x = 0; x < bitmap.Width; x++) {
                    float r = 0;
                    float g = 0;
                    float b = 0;
                    for (int j = y-range+1; j < y+range; j++) {
                        if (j < 0 || j >= bitmap.Height) {
                            r += colors[x+y*bitmap.Width].R*blurMatrix[Math.Abs(y-j)];
                            g += colors[x+y*bitmap.Width].G*blurMatrix[Math.Abs(y-j)];
                            b += colors[x+y*bitmap.Width].B*blurMatrix[Math.Abs(y-j)];
                        } else {
                            r += colors2[x+j*bitmap.Width].R*blurMatrix[Math.Abs(y-j)];
                            g += colors2[x+j*bitmap.Width].G*blurMatrix[Math.Abs(y-j)];
                            b += colors2[x+j*bitmap.Width].B*blurMatrix[Math.Abs(y-j)];
                        }
                    }
                    result.SetPixel(x, y, ColorConvertors.ClampRgbToColor((int) r, (int) g, (int) b));
                }    
            });
            return result.Bitmap;
        }
    }
}