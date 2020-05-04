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

        private NumericUpDown resizeWidthNumeric;
        private NumericUpDown resizeHeightNumeric;

        public bool Showing = false;

        public AdjustmentsWindow() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            statusText = this.FindControl<TextBlock>("Status");
            resizeWidthNumeric = this.FindControl<NumericUpDown>("ResizeWidth");
            resizeHeightNumeric = this.FindControl<NumericUpDown>("ResizeHeight");
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
                Bitmap bitmap = mainWindow.CurrentBitmap; 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnRotateLeftButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Rotating image";
                Bitmap bitmap = mainWindow.CurrentBitmap; 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnFlipXButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Flipping image";
                Bitmap bitmap = mainWindow.CurrentBitmap; 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private async void OnFlipYButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Flipping image";
                Bitmap bitmap = mainWindow.CurrentBitmap; 
                await Task.Run(() => bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY));
                mainWindow.ChangeMainImage(bitmap);
                statusText.Text = "";
            }
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs eventArgs) {
            Showing = false;
            Hide();
        }
    }
}