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
using System;

namespace PixelPalette {
    public class SelectColorWindow : Window {
        public Color CurrentColor {get; set;} = Color.Black;
        private NumericUpDown colorRNumeric;
        private NumericUpDown colorGNumeric;
        private NumericUpDown colorBNumeric;

        public SelectColorWindow() {
            InitializeComponent();
        }

        public SelectColorWindow(Color color) {
            InitializeComponent();
            colorRNumeric.Value = color.R;
            colorGNumeric.Value = color.G;
            colorBNumeric.Value = color.B;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            colorRNumeric = this.FindControl<NumericUpDown>("ColorR");
            colorGNumeric = this.FindControl<NumericUpDown>("ColorG");
            colorBNumeric = this.FindControl<NumericUpDown>("ColorB");
        }

        private void ReloadColor() {
            int r = (int) colorRNumeric.Value;
            int g = (int) colorGNumeric.Value;
            int b = (int) colorBNumeric.Value;
            CurrentColor = Color.FromArgb(r, g, b);
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs eventArgs) {
            ReloadColor();
            Close(CurrentColor);
        }
    }
}