using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
//using Avalonia.Media.Imaging;
using AvaloniaImage = Avalonia.Controls.Image;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using System.Drawing.Imaging;
using System.Drawing;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PixelPalette.Algorithm;

namespace PixelPalette {
    public class MainWindow : Window {
        private AvaloniaImage mainImage;
        private TextBlock imageSizeText;
        private TextBlock statusText;
        private Border imageBorder;

        private Button undoButton;
        private Button redoButton;

        private PaletteWindow paletteWindow;
        private DitherWindow ditherWindow;
        private AdjustmentsWindow adjustmentsWindow;
        
        private LinkedList<Bitmap> undoBitmaps = new LinkedList<Bitmap>();
        private LinkedList<Bitmap> redoBitmaps = new LinkedList<Bitmap>();
        public Bitmap InitialBitmap {get; set;} = null;
        public Bitmap CurrentBitmap {get; private set;} = null;

        public List<Color> ColorPalette {get {return paletteWindow.ColorPalette;}}

        public bool Working {get; set;} = false;

        public MainWindow() {
            InitializeComponent();
            paletteWindow = new PaletteWindow();
            paletteWindow.mainWindow = this;
            ditherWindow = new DitherWindow();
            ditherWindow.mainWindow = this;
            adjustmentsWindow = new AdjustmentsWindow();
            adjustmentsWindow.mainWindow = this;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            mainImage = this.FindControl<AvaloniaImage>("MainImage");
            imageBorder = this.FindControl<Border>("ImageBorder");
            imageSizeText = this.FindControl<TextBlock>("ImageSize");
            statusText = this.FindControl<TextBlock>("Status");
            undoButton = this.FindControl<Button>("UndoButton");
            redoButton = this.FindControl<Button>("RedoButton");
            undoButton.IsEnabled = false;
            redoButton.IsEnabled = false;
        }

        public async Task<string> GetOpenImagePath() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AllowMultiple = false;
            openFileDialog.Filters.Add(new FileDialogFilter() { Name = "Image", Extensions =  { "png", "jpg", "jpeg", "bmp" } });
            string[] result = await openFileDialog.ShowAsync(this);
            if (result == null || result.Length < 1) {
                return null;
            }
            return result[0];
        }

        public async Task<string> GetSaveImagePath() {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExtension = "png";
            saveFileDialog.Filters.Add(new FileDialogFilter() { Name = "Image", Extensions =  { "png", "jpg", "jpeg", "bmp" } });
            string result = await saveFileDialog.ShowAsync(this);
            if (result == null || result == "") {
                return null;
            }
            return result;
        }

        public void UndoImage() {
            if (undoBitmaps.Count == 0) {
                return;
            }
            if (CurrentBitmap != null) {
                redoBitmaps.AddLast(CurrentBitmap);
                redoButton.IsEnabled = true;
            }
            CurrentBitmap = undoBitmaps.Last();
            undoBitmaps.RemoveLast();
            ReloadMainImage();
            if (undoBitmaps.Count == 0) {
                undoButton.IsEnabled = false;
            }
        }

        public void RedoImage() {
            if (redoBitmaps.Count == 0) {
                return;
            }
            if (CurrentBitmap != null) {
                undoBitmaps.AddLast(CurrentBitmap);
                undoButton.IsEnabled = true;
            }
            CurrentBitmap = redoBitmaps.Last();
            redoBitmaps.RemoveLast();
            ReloadMainImage();
            if (redoBitmaps.Count == 0) {
                redoButton.IsEnabled = false;
            }
        }

        public void ChangeMainImage(Bitmap bitmap) {
            if (CurrentBitmap != null) {
                if (undoBitmaps.Count > 15) {
                    undoBitmaps.RemoveFirst();
                }
                undoBitmaps.AddLast(CurrentBitmap);
                undoButton.IsEnabled = true;
            }
            redoBitmaps.Clear();
            redoButton.IsEnabled = false;
            CurrentBitmap = bitmap;
            ReloadMainImage();
        }

        private void ReloadMainImage() {
            mainImage.Source = BitmapConvert.ConvertToAvaloniaBitmap(CurrentBitmap);
            UpdateMainImageStretch();
            imageSizeText.Text = $"{CurrentBitmap.Size.Width}x{CurrentBitmap.Size.Height}";
            if (adjustmentsWindow.lockRatioCheckBox.IsChecked.Value) {
                adjustmentsWindow.lockRatioCheckBox.IsChecked = false;
                adjustmentsWindow.resizeWidthNumeric.Value = CurrentBitmap.Size.Width;
                adjustmentsWindow.resizeHeightNumeric.Value = CurrentBitmap.Size.Height;
                adjustmentsWindow.lockRatioCheckBox.IsChecked = true;
                adjustmentsWindow.OnUpdateResizeRatio();
            }
        }

        private void UpdateMainImageStretch() {
            if (CurrentBitmap.Width > imageBorder.Bounds.Width || CurrentBitmap.Height > imageBorder.Bounds.Height) {
                mainImage.Stretch = Avalonia.Media.Stretch.Uniform;
            } else {
                mainImage.Stretch = Avalonia.Media.Stretch.None;
            }
        }

        private async void OnOpenFileButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (Working) {
                return;
            }
            string path = await GetOpenImagePath();
            if (path != null && path != "") {
                InitialBitmap = new Bitmap(path);
                CurrentBitmap = InitialBitmap;
                undoBitmaps.Clear();
                undoButton.IsEnabled = false;
                redoBitmaps.Clear();
                redoButton.IsEnabled = false;
                ReloadMainImage();
            }
        }

        private async void OnSaveFileButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (CurrentBitmap == null || Working) {
                return;
            }
            string path = await GetSaveImagePath();
            if (path != null && path != "") {
                if (!path.Contains(".")) {
                    path += ".png";
                }
                CurrentBitmap.Save(path);
            }
        }

        private void OnUndoButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (Working) {
                return;
            }
            UndoImage();
        }

        private void OnRedoButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (Working) {
                return;
            }
            RedoImage();
        }

        private void OnRestoreButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (Working) {
                return;
            }
            if (CurrentBitmap != null && InitialBitmap != CurrentBitmap) {
                Working = true;
                statusText.Text = "Restoring image";
                ChangeMainImage(InitialBitmap);
                statusText.Text = "";
                Working = false;
            }
        }

        private void OnDitherButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (ditherWindow.Showing) {
                return;
            }
            ditherWindow.Showing = true;
            ditherWindow.Show();
        }

        private void OnPaletteButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (paletteWindow.Showing) {
                return;
            }
            paletteWindow.Showing = true;
            paletteWindow.Show();
        }

        private void OnAdjustmentsButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (adjustmentsWindow.Showing) {
                return;
            }
            adjustmentsWindow.Showing = true;
            adjustmentsWindow.Show();
        }
    }
}