using System;
using SkiaSharp;
using Avalonia;
using Avalonia.Visuals.Media.Imaging;
//using Avalonia.Media.Imaging;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PixelPalette.Algorithm {
    public static class BitmapConvert {
        public static AvaloniaBitmap ConvertToAvaloniaBitmap(Bitmap bitmap) {
            using (MemoryStream memoryStream = new MemoryStream()) {
                bitmap.Save(memoryStream, ImageFormat.Jpeg);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var a = memoryStream.Length;
                return new AvaloniaBitmap(new StreamReader(memoryStream).BaseStream);
            }
        }

        //from https://stackoverflow.com/questions/12168654/image-processing-with-lockbits-alternative-to-getpixel
        public static byte[] Array1DFromBitmap(Bitmap bitmap) {
            if (bitmap == null) throw new NullReferenceException("Bitmap is null");

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = data.Scan0;

            //declare an array to hold the bytes of the bitmap
            int numBytes = data.Stride * bitmap.Height;
            byte[] bytes = new byte[numBytes];

            //copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, numBytes);

            bitmap.UnlockBits(data);

            return bytes;           
        }

        public static Color[] ColorArrayFromBitmap(Bitmap bitmap) {
            byte[] bytes = Array1DFromBitmap(bitmap);
            Color[] colors = new Color[bytes.Length/3];
            for (int i = 0; i < bytes.Length/3; i++) {
                colors[i] = Color.FromArgb(bytes[i*3+2], bytes[i*3+1], bytes[i*3]);
            }
            return colors;
        }

        public static (int R, int G, int B)[] IntArrayFromBitmap(Bitmap bitmap) {
            byte[] bytes = Array1DFromBitmap(bitmap);
            (int R, int G, int B)[] colors = new (int R, int G, int B)[bytes.Length/3];
            for (int i = 0; i < bytes.Length/3; i++) {
                colors[i] = (bytes[i*3+2], bytes[i*3+1], bytes[i*3]);
            }
            return colors;
        }
    }
}