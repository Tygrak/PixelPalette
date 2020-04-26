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

namespace PixelPalette {
    public class PaletteWindow : Window {
        public MainWindow mainWindow;
        private TextBlock statusText;
        private ListBox colorsListBox;

        public bool Showing = false;
        
        public List<Color> ColorPalette {get; set;}

        public PaletteWindow() {
            ColorPalette = new List<Color> {Color.Red, Color.Green, Color.FromArgb(20, 20, 180), Color.Black, Color.White};
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = this;
            colorsListBox = this.FindControl<ListBox>("ColorsListBox");
            ReloadPaletteItems();
            statusText = this.FindControl<TextBlock>("Status");
            Closing += (s, e) => {
                Hide();
                Showing = false;
                e.Cancel = true;
            };
        }

        private async void GeneratePalette() {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Generating Palette";
                ColorPalette = await Task.Run(() => ColorHelpers.MedianCut(mainWindow.CurrentBitmap, 8));
                ReloadPaletteItems();
                //ReloadMainImage();
                statusText.Text = "";
            }
        }

        private void ReloadPaletteItems() {
            colorsListBox.Items = ColorPalette.Select(c => new ColorPaletteItem(c));
        }

        private void OnMedianCutButtonClick(object sender, RoutedEventArgs eventArgs) {
            GeneratePalette();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs eventArgs) {
            Showing = false;
            Hide();
        }
    }
}