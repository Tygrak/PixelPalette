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
    public class AdjustmentsWindow : Window {
        public MainWindow mainWindow;
        private TextBlock statusText;
        private TextBlock compareErrorText;

        private NumericUpDown resizeWidthNumeric;
        private NumericUpDown resizeHeightNumeric;
        private NumericUpDown hueNumeric;
        private NumericUpDown saturationNumeric;
        private NumericUpDown lightnessNumeric;
        private NumericUpDown blurSigmaNumeric;

        public bool Showing = false;

        public AdjustmentsWindow() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            statusText = this.FindControl<TextBlock>("Status");
            compareErrorText = this.FindControl<TextBlock>("CompareErrorResult");
            resizeWidthNumeric = this.FindControl<NumericUpDown>("ResizeWidth");
            resizeHeightNumeric = this.FindControl<NumericUpDown>("ResizeHeight");
            hueNumeric = this.FindControl<NumericUpDown>("Hue");
            saturationNumeric = this.FindControl<NumericUpDown>("Saturation");
            lightnessNumeric = this.FindControl<NumericUpDown>("Lightness");
            blurSigmaNumeric = this.FindControl<NumericUpDown>("BlurSigma");
            Closing += (s, e) => {
                Hide();
                Showing = false;
                e.Cancel = true;
            };
        }

        private async void OnResizeButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Resizing image";
                int width = (int) resizeWidthNumeric.Value;
                int height = (int) resizeHeightNumeric.Value;
                Bitmap bitmap = await Task.Run(() => new Bitmap(mainWindow.CurrentBitmap, width, height));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnRotateRightButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Rotating image";
                Bitmap bitmap = new Bitmap(mainWindow.CurrentBitmap); 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnRotateLeftButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Rotating image";
                Bitmap bitmap = new Bitmap(mainWindow.CurrentBitmap); 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnFlipXButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Flipping image";
                Bitmap bitmap = new Bitmap(mainWindow.CurrentBitmap); 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnFlipYButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Flipping image";
                Bitmap bitmap = new Bitmap(mainWindow.CurrentBitmap); 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnShiftHslButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Adjusting image";
                float hue = (float) hueNumeric.Value;
                float saturation = (float) (saturationNumeric.Value/100);
                float lightness = (float) (lightnessNumeric.Value/100);
                mainWindow.ChangeMainImage(await Task.Run(() => ColorAdjustments.ShiftHsl(mainWindow.CurrentBitmap, hue, saturation, lightness)));
                statusText.Text = "";
            }
        }

        private async void OnBlurButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Bluring image";
                float sigma = (float) blurSigmaNumeric.Value;
                mainWindow.ChangeMainImage(await Task.Run(() => ColorAdjustments.BlurImage(mainWindow.CurrentBitmap, sigma)));
                statusText.Text = "";
            }
        }

        private async void OnCompareButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Comparing images";
                double result = await Task.Run(() => ColorHelpers.CalculateAverageError(mainWindow.CurrentBitmap, mainWindow.InitialBitmap));
                compareErrorText.Text = result.ToString();
                statusText.Text = "";
            }
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs eventArgs) {
            Showing = false;
            Hide();
        }
    }
}