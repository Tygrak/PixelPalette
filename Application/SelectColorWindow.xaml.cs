using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using PixelPalette.Algorithm;
using System.Text.RegularExpressions;

namespace PixelPalette {
    public class SelectColorWindow : Window {
        public Color CurrentColor {get; set;} = Color.Black;
        private ColorPaletteItem currentColorItem = new ColorPaletteItem(Color.Black);
        private NumericUpDown colorRNumeric;
        private NumericUpDown colorGNumeric;
        private NumericUpDown colorBNumeric;
        private NumericUpDown colorHNumeric;
        private NumericUpDown colorSNumeric;
        private NumericUpDown colorLNumeric;
        private TextBox colorHexTextBox;
        private StackPanel colorStackPanel;

        private bool colorChanged = false;

        public SelectColorWindow() {
            InitializeComponent();
        }

        public SelectColorWindow(Color color) {
            InitializeComponent();
            colorRNumeric.Value = color.R;
            colorGNumeric.Value = color.G;
            colorBNumeric.Value = color.B;
            ReloadColorRgb();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            colorRNumeric = this.FindControl<NumericUpDown>("ColorR");
            colorRNumeric.ValueChanged += (o, e) => {ReloadColorRgb();};
            colorGNumeric = this.FindControl<NumericUpDown>("ColorG");
            colorGNumeric.ValueChanged += (o, e) => {ReloadColorRgb();};
            colorBNumeric = this.FindControl<NumericUpDown>("ColorB");
            colorBNumeric.ValueChanged += (o, e) => {ReloadColorRgb();};
            colorHNumeric = this.FindControl<NumericUpDown>("ColorH");
            colorHNumeric.ValueChanged += (o, e) => {ReloadColorHsl();};
            colorSNumeric = this.FindControl<NumericUpDown>("ColorS");
            colorSNumeric.ValueChanged += (o, e) => {ReloadColorHsl();};
            colorLNumeric = this.FindControl<NumericUpDown>("ColorL");
            colorLNumeric.ValueChanged += (o, e) => {ReloadColorHsl();};
            colorHexTextBox = this.FindControl<TextBox>("ColorHex");
            colorHexTextBox.LostFocus += (o, e) => {ReloadColorHex();};
            colorHexTextBox.KeyUp += (o, e) => {ReloadColorHex();};
            colorStackPanel = this.FindControl<StackPanel>("ColorStackPanel");
            colorStackPanel.DataContext = currentColorItem;
        }

        private void ReloadColorRgb() {
            if (colorChanged) {
                return;
            }
            colorChanged = true;
            int r = (int) colorRNumeric.Value;
            int g = (int) colorGNumeric.Value;
            int b = (int) colorBNumeric.Value;
            CurrentColor = Color.FromArgb(r, g, b);
            var hsl = ColorConvertors.ColorToHsl(CurrentColor);
            colorHNumeric.Value = hsl.H;
            colorSNumeric.Value = hsl.S;
            colorLNumeric.Value = hsl.L;
            colorHexTextBox.Text = ColorConvertors.ColorToHex(CurrentColor);
            ReloadColorMain();
        }

        private void ReloadColorHsl() {
            if (colorChanged) {
                return;
            }
            colorChanged = true;
            float h = (float) (colorHNumeric.Value%360);
            float s = (float) colorSNumeric.Value;
            float l = (float) colorLNumeric.Value;
            CurrentColor = ColorConvertors.HslToColor(h, s, l);
            colorRNumeric.Value = CurrentColor.R;
            colorGNumeric.Value = CurrentColor.G;
            colorBNumeric.Value = CurrentColor.B;
            colorHexTextBox.Text = ColorConvertors.ColorToHex(CurrentColor);
            ReloadColorMain();
        }

        private void ReloadColorHex() {
            if (colorChanged || !Regex.IsMatch(colorHexTextBox.Text, @"#?[0-9a-fA-F]{6}")) {
                return;
            }
            colorChanged = true;
            CurrentColor = ColorConvertors.HexToColor(colorHexTextBox.Text);
            colorRNumeric.Value = CurrentColor.R;
            colorGNumeric.Value = CurrentColor.G;
            colorBNumeric.Value = CurrentColor.B;
            var hsl = ColorConvertors.ColorToHsl(CurrentColor);
            colorHNumeric.Value = hsl.H;
            colorSNumeric.Value = hsl.S;
            colorLNumeric.Value = hsl.L;
            ReloadColorMain();
        }

        private void ReloadColorMain() {
            colorChanged = false;
            currentColorItem = new ColorPaletteItem(CurrentColor);
            colorStackPanel.DataContext = currentColorItem;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs eventArgs) {
            Close(CurrentColor);
        }
    }
}