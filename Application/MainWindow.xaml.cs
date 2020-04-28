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
using PixelPalette.Algorithm;

namespace PixelPalette {
    public class MainWindow : Window {
        private AvaloniaImage mainImage;
        private TextBlock imageSizeText;
        private TextBlock statusText;

        private PaletteWindow paletteWindow;
        private DitherWindow ditherWindow;

        public Bitmap InitialBitmap {get; set;} = null;
        public Bitmap CurrentBitmap {get; set;} = null;

        public List<Color> ColorPalette {get {return paletteWindow.ColorPalette;}}

        public App app;

        public MainWindow() {
            InitializeComponent();
            paletteWindow = new PaletteWindow();
            paletteWindow.mainWindow = this;
            ditherWindow = new DitherWindow();
            ditherWindow.mainWindow = this;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            mainImage = this.FindControl<AvaloniaImage>("MainImage");
            imageSizeText = this.FindControl<TextBlock>("ImageSize");
            statusText = this.FindControl<TextBlock>("Status");
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

        public void ReloadMainImage() {
            mainImage.Source = BitmapConvert.ConvertToAvaloniaBitmap(CurrentBitmap);
            imageSizeText.Text = $"{InitialBitmap.Size.Width}x{InitialBitmap.Size.Height}";
        }

        private async void OnOpenFileButtonClick(object sender, RoutedEventArgs eventArgs) {
            string path = await GetOpenImagePath();
            if (path != null && path != "") {
                InitialBitmap = new Bitmap(path);
                CurrentBitmap = InitialBitmap;
                ReloadMainImage();
            }
        }

        private async void OnSaveFileButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (CurrentBitmap == null) {
                return;
            }
            string path = await GetSaveImagePath();
            if (path != null && path != "") {
                CurrentBitmap.Save(path);
            }
        }

        private void OnRestoreButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (CurrentBitmap != null) {
                statusText.Text = "Restoring image";
                CurrentBitmap = InitialBitmap; 
                ReloadMainImage();
                statusText.Text = "";
            }
        }

        private void OnDitherButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (ditherWindow.Showing) {
                return;
            }
            ditherWindow.Showing = true;
            ditherWindow.ShowDialog(this);
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