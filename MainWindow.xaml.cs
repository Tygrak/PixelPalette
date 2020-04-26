using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
//using Avalonia.Media.Imaging;
using AvaloniaImage = Avalonia.Controls.Image;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using System.Drawing.Imaging;
using System.Drawing;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PixelPalette {
    public class MainWindow : Window {
        private Ditherer ditherer = new Ditherer();

        private AvaloniaImage mainImage;
        private TextBlock imageSizeText;
        private TextBlock statusText;

        private PaletteWindow paletteWindow;

        public Bitmap InitialBitmap {get; set;} = null;
        public Bitmap CurrentBitmap {get; set;} = null;

        public App app;

        public MainWindow() {
            InitializeComponent();
            paletteWindow = new PaletteWindow();
            paletteWindow.mainWindow = this;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            mainImage = this.FindControl<AvaloniaImage>("MainImage");
            imageSizeText = this.FindControl<TextBlock>("ImageSize");
            statusText = this.FindControl<TextBlock>("Status");
            Closing += (s, e) => {
                //System.Environment.Exit(0);
            };
        }

        private async Task<string> GetOpenImagePath() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AllowMultiple = false;
            openFileDialog.Filters.Add(new FileDialogFilter() { Name = "Image", Extensions =  { "png", "jpg", "jpeg", "bmp" } });
            string[] result = await openFileDialog.ShowAsync(this);
            if (result == null || result.Length < 1) {
                return null;
            }
            return result[0];
        }

        private async void LoadImage() {
            string path = await GetOpenImagePath();
            if (path != null && path != "") {
                InitialBitmap = new Bitmap(path);
                CurrentBitmap = InitialBitmap;
                ReloadMainImage();
            }
        }

        private async Task<string> GetSaveImagePath() {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExtension = "png";
            saveFileDialog.Filters.Add(new FileDialogFilter() { Name = "Image", Extensions =  { "png", "jpg", "jpeg", "bmp" } });
            string result = await saveFileDialog.ShowAsync(this);
            if (result == null || result == "") {
                return null;
            }
            return result;
        }

        private async void SaveImage() {
            if (CurrentBitmap == null) {
                return;
            }
            string path = await GetSaveImagePath();
            if (path != null && path != "") {
                CurrentBitmap.Save(path);
            }
        }

        private void RestoreImage() {
            if (CurrentBitmap != null) {
                statusText.Text = "Restoring image";
                CurrentBitmap = InitialBitmap; 
                ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void SortImage() {
            if (CurrentBitmap != null) {
                statusText.Text = "Sorting image";
                CurrentBitmap = await Task.Run(() => ditherer.SortColors(CurrentBitmap, paletteWindow.ColorPalette.ToArray())); 
                ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void ThresholdImage() {
            if (CurrentBitmap != null) {
                statusText.Text = "Thresholding image";
                CurrentBitmap = await Task.Run(() => ditherer.ClosestColor(CurrentBitmap, paletteWindow.ColorPalette.ToArray())); 
                ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void DitherImage() {
            if (CurrentBitmap != null) {
                statusText.Text = "Dithering image";
                CurrentBitmap = await Task.Run(() => ditherer.RandomDither(CurrentBitmap, paletteWindow.ColorPalette.ToArray())); 
                ReloadMainImage();
                statusText.Text = "";
            }
        }

        private void ReloadMainImage() {
            mainImage.Source = BitmapConvert.ConvertToAvaloniaBitmap(CurrentBitmap);
            imageSizeText.Text = $"{InitialBitmap.Size.Width}x{InitialBitmap.Size.Height}";
        }

        private void OnOpenFileButtonClick(object sender, RoutedEventArgs eventArgs) {
            LoadImage();
        }

        private void OnSaveFileButtonClick(object sender, RoutedEventArgs eventArgs) {
            SaveImage();
        }

        private void OnRestoreButtonClick(object sender, RoutedEventArgs eventArgs) {
            RestoreImage();
        }

        private void OnSortButtonClick(object sender, RoutedEventArgs eventArgs) {
            SortImage();
        }

        private void OnThresholdButtonClick(object sender, RoutedEventArgs eventArgs) {
            ThresholdImage();
        }

        private void OnDitherButtonClick(object sender, RoutedEventArgs eventArgs) {
            DitherImage();
        }

        private void OnPaletteButtonClick(object sender, RoutedEventArgs eventArgs) {
            //GeneratePalette();
            if (paletteWindow.Showing) {
                return;
            }
            paletteWindow.Showing = true;
            paletteWindow.ShowDialog(this);
        }
    }
}