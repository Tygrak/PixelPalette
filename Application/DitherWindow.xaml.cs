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

namespace PixelPalette {
    public class DitherWindow : Window {
        public MainWindow mainWindow;
        private TextBlock statusText;

        private NumericUpDown randomBiasNumeric;
        private NumericUpDown[] orderedMatrixNumeric = new NumericUpDown[16];
        private NumericUpDown[] oneRowErrorNumeric = new NumericUpDown[4];
        private NumericUpDown oneRowErrorMultiplierNumeric;
        private NumericUpDown[] twoRowErrorNumeric = new NumericUpDown[12];
        private NumericUpDown twoRowErrorMultiplierNumeric;

        public bool Showing = false;

        public DitherWindow() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = this;
            statusText = this.FindControl<TextBlock>("Status");
            randomBiasNumeric = this.FindControl<NumericUpDown>("RandomBias");
            oneRowErrorNumeric[0] = this.FindControl<NumericUpDown>("OneRowError20");
            oneRowErrorNumeric[1] = this.FindControl<NumericUpDown>("OneRowError01");
            oneRowErrorNumeric[2] = this.FindControl<NumericUpDown>("OneRowError11");
            oneRowErrorNumeric[3] = this.FindControl<NumericUpDown>("OneRowError21");
            oneRowErrorMultiplierNumeric = this.FindControl<NumericUpDown>("OneRowErrorMultiplier");
            twoRowErrorNumeric[0] = this.FindControl<NumericUpDown>("TwoRowError30");
            twoRowErrorNumeric[1] = this.FindControl<NumericUpDown>("TwoRowError40");
            twoRowErrorNumeric[2] = this.FindControl<NumericUpDown>("TwoRowError01");
            twoRowErrorNumeric[3] = this.FindControl<NumericUpDown>("TwoRowError11");
            twoRowErrorNumeric[4] = this.FindControl<NumericUpDown>("TwoRowError21");
            twoRowErrorNumeric[5] = this.FindControl<NumericUpDown>("TwoRowError31");
            twoRowErrorNumeric[6] = this.FindControl<NumericUpDown>("TwoRowError41");
            twoRowErrorNumeric[7] = this.FindControl<NumericUpDown>("TwoRowError02");
            twoRowErrorNumeric[8] = this.FindControl<NumericUpDown>("TwoRowError12");
            twoRowErrorNumeric[9] = this.FindControl<NumericUpDown>("TwoRowError22");
            twoRowErrorNumeric[10] = this.FindControl<NumericUpDown>("TwoRowError32");
            twoRowErrorNumeric[11] = this.FindControl<NumericUpDown>("TwoRowError42");
            twoRowErrorMultiplierNumeric = this.FindControl<NumericUpDown>("TwoRowErrorMultiplier");
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    orderedMatrixNumeric[i+j*4] = this.FindControl<NumericUpDown>($"OrderedMatrix{i}{j}");
                }
            }
            Closing += (s, e) => {
                Hide();
                Showing = false;
                e.Cancel = true;
            };
        }

        private async void OnThresholdButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null && mainWindow.ColorPalette.Count > 0) {
                statusText.Text = "Thresholding image";
                mainWindow.CurrentBitmap = await Task.Run(() => Ditherer.ClosestColor(mainWindow.CurrentBitmap, mainWindow.ColorPalette.ToArray())); 
                mainWindow.ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void OnRandomButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null && mainWindow.ColorPalette.Count > 0) {
                statusText.Text = "Dithering image";
                float bias = (float) randomBiasNumeric.Value;
                mainWindow.CurrentBitmap = await Task.Run(() => Ditherer.RandomDither(mainWindow.CurrentBitmap, mainWindow.ColorPalette.ToArray(), bias)); 
                mainWindow.ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void OnOrderedButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null && mainWindow.ColorPalette.Count > 0) {
                statusText.Text = "Dithering image";
                float[,] matrix = new float[4, 4];
                for (int i = 0; i < orderedMatrixNumeric.Length; i++) {
                    matrix[i%4, i/4] = ((float) orderedMatrixNumeric[i].Value)/orderedMatrixNumeric.Length;
                }
                //todo: configurable size
                mainWindow.CurrentBitmap = await Task.Run(() => Ditherer.OrderedDither(mainWindow.CurrentBitmap, mainWindow.ColorPalette.ToArray(), matrix)); 
                mainWindow.ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void OnFloSteinButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null && mainWindow.ColorPalette.Count > 0) {
                statusText.Text = "Dithering image";
                float[] errorMatrix = new float[4];
                for (int i = 0; i < errorMatrix.Length; i++) {
                    errorMatrix[i] = (float) (oneRowErrorNumeric[i].Value/oneRowErrorMultiplierNumeric.Value);
                }
                mainWindow.CurrentBitmap = await Task.Run(() => Ditherer.FloydSteinbergDither(mainWindow.CurrentBitmap, mainWindow.ColorPalette.ToArray(), errorMatrix)); 
                mainWindow.ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void OnMinAvgErrButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null && mainWindow.ColorPalette.Count > 0) {
                statusText.Text = "Dithering image";
                float[] errorMatrix = new float[12];
                for (int i = 0; i < errorMatrix.Length; i++) {
                    errorMatrix[i] = (float) (twoRowErrorNumeric[i].Value/twoRowErrorMultiplierNumeric.Value);
                }
                mainWindow.CurrentBitmap = await Task.Run(() => Ditherer.MinAvgErrDither(mainWindow.CurrentBitmap, mainWindow.ColorPalette.ToArray(), errorMatrix)); 
                mainWindow.ReloadMainImage();
                statusText.Text = "";
            }
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs eventArgs) {
            Showing = false;
            Hide();
        }
    }
}