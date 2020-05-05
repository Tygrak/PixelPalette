using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace PixelPalette.Algorithm {
    public static class ColorAdjustments {
        public static Bitmap ShiftHsl(Bitmap bitmap, float hueShift, float saturation, float lightness) {
            Color[] colors = BitmapConvert.ColorArrayFromBitmap(bitmap);
            DirectBitmap result = new DirectBitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    var hsl = ColorHelpers.ColorToHsl(colors[x+y*bitmap.Width]);
                    hsl.H = MathF.Max(hsl.H+hueShift, 0)%360;
                    hsl.S = MathF.Min(MathF.Max((hsl.S)*(1+saturation), 0), 1);
                    hsl.L = MathF.Min(MathF.Max((hsl.L)*(1+lightness), 0), 1);
                    result.SetPixel(x, y, ColorHelpers.HslToColor(hsl));
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
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    float r = 0;
                    float g = 0;
                    float b = 0;
                    for (int i = Math.Max(x-range+1, 0); i < Math.Min(x+range, bitmap.Width); i++) {
                        r += colors[i+y*bitmap.Width].R*blurMatrix[Math.Abs(x-i)];
                        g += colors[i+y*bitmap.Width].G*blurMatrix[Math.Abs(x-i)];
                        b += colors[i+y*bitmap.Width].B*blurMatrix[Math.Abs(x-i)];
                    }
                    colors2[x+y*bitmap.Width] = ColorHelpers.ClampRgbToColor((int) r, (int) g, (int) b);
                }    
            }
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    float r = 0;
                    float g = 0;
                    float b = 0;
                    for (int j = Math.Max(y-range+1, 0); j < Math.Min(y+range, bitmap.Height); j++) {
                        r += colors2[x+j*bitmap.Width].R*blurMatrix[Math.Abs(y-j)];
                        g += colors2[x+j*bitmap.Width].G*blurMatrix[Math.Abs(y-j)];
                        b += colors2[x+j*bitmap.Width].B*blurMatrix[Math.Abs(y-j)];
                    }
                    result.SetPixel(x, y, ColorHelpers.ClampRgbToColor((int) r, (int) g, (int) b));
                }    
            }
            return result.Bitmap;
        }
    }
}