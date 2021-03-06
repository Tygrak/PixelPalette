using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaBrush = Avalonia.Media.SolidColorBrush;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace PixelPalette.Algorithm {
    public class ColorPaletteItem {
        public Color color;
        public string Hex {
            get {
                return ColorConvertors.ColorToHex(color);
            }
        }
        public AvaloniaBrush Brush {
            get {
                return new AvaloniaBrush(AvaloniaColor.FromRgb(color.R, color.G, color.B));
            }
        }

        public ColorPaletteItem(Color color) {
            this.color = color;
        }
    }
}