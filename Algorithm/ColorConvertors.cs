using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace PixelPalette.Algorithm {
    public static class ColorConvertors {
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

        //todo: fix and test
        //rgb to xyz and to lab modified from: https://www.codeproject.com/Articles/19045/Manipulating-colors-in-NET-Part-1?msg=5197888#intro
        public static readonly (double x, double y, double z) XyzD65 = (0.9505, 1.0, 1.0890);

        public static (double x, double y, double z) RgbToXyz(int r, int g, int b) {
            double rLinear = (double)r/255d;
            double gLinear = (double)g/255d;
            double bLinear = (double)b/255d;
            double sR = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055)/(1 + 0.055), 2.2) : (rLinear/12.92);
            double sG = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055)/(1 + 0.055), 2.2) : (gLinear/12.92);
            double sB = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055)/(1 + 0.055), 2.2) : (bLinear/12.92);
            return (sR*0.4124 + sG*0.3576 + sB*0.1805, sR*0.2126 + sG*0.7152 + sB*0.0722, sR*0.0193 + sG*0.1192 + sB*0.9505);
        }

        public static (double l, double a, double b) RgbToLab(int r, int g, int b) {
            var xyz = RgbToXyz(r, g, b);
            return XyzToLab(xyz.x, xyz.y, xyz.z);
        }

        private static double FXyz(double t) {
            return ((t > 0.008856)? Math.Pow(t, (1.0/3.0)) : (7.787*t + 16.0/116.0));
        }

        public static (double l, double a, double b) XyzToLab(double x, double y, double z) {
            (double l, double a, double b) lab = (0, 0, 0);
            lab.l = 116.0 * FXyz(y/XyzD65.y) - 16;
            lab.a = 500.0 * (FXyz(x/XyzD65.x) - FXyz(y/XyzD65.y));
            lab.b = 200.0 * (FXyz(y/XyzD65.y) - FXyz(z/XyzD65.z));
            return lab;
        }

        public static Color LabToRgb(double l, double a, double b) {
            var xyz = LabToXyz(l, a, b);
            return XyzToRgb(xyz.x, xyz.y, xyz.z);
        }

        public static (double x, double y, double z) LabToXyz(double l, double a, double b) {
            double delta = 6d/29d;
            double fy = (l+16)/116d;
            double fx = fy + (a/500d);
            double fz = fy - (b/200d);
            return (
                (fx > delta)? XyzD65.x * (fx*fx*fx) : (fx - 16.0/116.0)*3*(delta*delta)*XyzD65.x,
                (fy > delta)? XyzD65.y * (fy*fy*fy) : (fy - 16.0/116.0)*3*(delta*delta)*XyzD65.y,
                (fz > delta)? XyzD65.z * (fz*fz*fz) : (fz - 16.0/116.0)*3*(delta*delta)*XyzD65.z
            );
        }

        public static Color XyzToRgb(double x, double y, double z) {
            double[] Clinear = new double[3];
            Clinear[0] = x*3.2410 - y*1.5374 - z*0.4986; // red
            Clinear[1] = -x*0.9692 + y*1.8760 - z*0.0416; // green
            Clinear[2] = x*0.0556 - y*0.2040 + z*1.0570; // blue
            for(int i=0; i<3; i++) {
                Clinear[i] = (Clinear[i]<=0.0031308)? 12.92*Clinear[i] : (
                    1+0.055)* Math.Pow(Clinear[i], (1.0/2.4)) - 0.055;
            }
            return Color.FromArgb((int) Math.Round(Clinear[0]*255.0), (int) Math.Round(Clinear[1]*255.0), (int) Math.Round(Clinear[2]*255.0));
        }

        public static Color ClampRgbToColor(int r, int g, int b) {
            return Color.FromArgb(Math.Max(0, Math.Min(255, r)), Math.Max(0, Math.Min(255, g)), Math.Max(0, Math.Min(255, b)));
        }
    }
}