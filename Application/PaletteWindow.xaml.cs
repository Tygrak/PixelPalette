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
    public class PaletteWindow : Window {
        public MainWindow mainWindow;
        private TextBlock statusText;
        private ListBox colorsListBox;

        private NumericUpDown medianCutAmountNumeric;
        private NumericUpDown kMeansAmountNumeric;
        private NumericUpDown kMeansStepsNumeric;
        private NumericUpDown duoToneAmountNumeric;
        private NumericUpDown duoToneHueNumeric;
        private NumericUpDown duoToneSaturationNumeric;
        private TextBox importExportSeparatorTextBox;

        private string importExportSeparator {
            get {
                string separator = importExportSeparatorTextBox.Text;
                separator = separator.Replace(@"\n", "\n");
                return separator;
            }
        }

        public bool Showing = false;
        
        public List<Color> ColorPalette {get; set;}

        public PaletteWindow() {
            ColorPalette = new List<Color> {Color.Black, Color.White};
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = this;
            colorsListBox = this.FindControl<ListBox>("ColorsListBox");
            colorsListBox.DoubleTapped += OnColorListBoxDoubleTapped;
            ReloadPaletteItems();
            statusText = this.FindControl<TextBlock>("Status");
            medianCutAmountNumeric = this.FindControl<NumericUpDown>("MedianCutColorAmount");
            kMeansAmountNumeric = this.FindControl<NumericUpDown>("KMeansColorAmount");
            kMeansStepsNumeric = this.FindControl<NumericUpDown>("KMeansStepAmount");
            duoToneAmountNumeric = this.FindControl<NumericUpDown>("DuoToneColorAmount");
            duoToneHueNumeric = this.FindControl<NumericUpDown>("DuoToneHue");
            duoToneSaturationNumeric = this.FindControl<NumericUpDown>("DuoToneSaturation");
            importExportSeparatorTextBox = this.FindControl<TextBox>("ImportSeparator");
            Closing += (s, e) => {
                Hide();
                Showing = false;
                e.Cancel = true;
            };
        }

        private async void OnColorListBoxDoubleTapped(object sender, RoutedEventArgs eventArgs) {
            if (colorsListBox.SelectedItem == null) {
                return;
            }
            Color selectedColor = ((ColorPaletteItem) colorsListBox.SelectedItem).color;
            SelectColorWindow dialog = new SelectColorWindow(selectedColor);
            Color result = await dialog.ShowDialog<Color>(this);
            ColorPalette[ColorPalette.IndexOf(selectedColor)] = result;
            ReloadPaletteItems();
        }

        private void ReloadPaletteItems() {
            colorsListBox.Items = ColorPalette.Select(c => new ColorPaletteItem(c));
        }

        private async void OnMedianCutButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Generating Palette";
                int amount = (int) medianCutAmountNumeric.Value;
                ColorPalette = await Task.Run(() => PaletteGeneration.MedianCut(mainWindow.CurrentBitmap, amount));
                ReloadPaletteItems();
                statusText.Text = "";
            }
        }

        private async void OnKMeansButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Generating Palette";
                int amount = (int) kMeansAmountNumeric.Value;
                int steps = (int) kMeansStepsNumeric.Value;
                ColorPalette = await Task.Run(() => PaletteGeneration.KMeans(mainWindow.CurrentBitmap, amount, steps));
                ReloadPaletteItems();
                statusText.Text = "";
            }
        }

        private void OnClearPaletteButtonClick(object sender, RoutedEventArgs eventArgs) {
            ColorPalette.Clear();
            ReloadPaletteItems();
        }

        private async void OnAddColorButtonClick(object sender, RoutedEventArgs eventArgs) {
            SelectColorWindow dialog = new SelectColorWindow();
            Color result = await dialog.ShowDialog<Color>(this);
            ColorPalette.Add(result);
            ReloadPaletteItems();
        }

        private void OnRemoveColorButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (colorsListBox.SelectedItem != null) {
                Color selectedColor = ((ColorPaletteItem) colorsListBox.SelectedItem).color;
                ColorPalette.Remove(selectedColor);
                ReloadPaletteItems();
            }
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs eventArgs) {
            Showing = false;
            Hide();
        }

        private async void OnSortButtonClick(object sender, RoutedEventArgs eventArgs) {
            if (mainWindow.CurrentBitmap != null) {
                statusText.Text = "Sorting image";
                int amount = (int) medianCutAmountNumeric.Value;
                mainWindow.CurrentBitmap = await Task.Run(() => PaletteGeneration.SortColors(mainWindow.CurrentBitmap, amount)); 
                mainWindow.ReloadMainImage();
                statusText.Text = "";
            }
        }

        private async void OnDuoTonePickColorButtonClick(object sender, RoutedEventArgs eventArgs) {
            SelectColorWindow dialog = new SelectColorWindow();
            Color result = await dialog.ShowDialog<Color>(this);
            var hslColor = ColorHelpers.ColorToHsl(result);
            duoToneHueNumeric.Value = hslColor.H;
            duoToneSaturationNumeric.Value = hslColor.S;
        }

        private async void OnDuoToneButtonClick(object sender, RoutedEventArgs eventArgs) {
            statusText.Text = "Generating Palette";
            int amount = (int) duoToneAmountNumeric.Value;
            float hue = (float) (duoToneHueNumeric.Value%360);
            float saturation = (float) duoToneSaturationNumeric.Value;
            ColorPalette = await Task.Run(() => PaletteGeneration.FakeDuoTone(amount, hue, saturation));
            ReloadPaletteItems();
            statusText.Text = "";
        }

        private async void OnImportButtonClick(object sender, RoutedEventArgs eventArgs) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AllowMultiple = false;
            openFileDialog.Filters.Add(new FileDialogFilter() {Name = "Palette File", Extensions =  {"palette", "txt"}});
            string[] path = await openFileDialog.ShowAsync(this);
            if (path == null || path.Length < 1) {
                return;
            }
            string colors = File.ReadAllText(path[0]);
            ColorPalette = colors.Split(importExportSeparator).Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Select(s => ColorHelpers.HexToColor(s)).Distinct().ToList();
            ReloadPaletteItems();
        }

        private async void OnExportButtonClick(object sender, RoutedEventArgs eventArgs) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExtension = "palette";
            saveFileDialog.Filters.Add(new FileDialogFilter() {Name = "Palette File", Extensions =  {"palette", "txt"}});
            string path = await saveFileDialog.ShowAsync(this);
            if (path == null || path == "") {
                return;
            }
            path = path.Contains('.') ? path : path+".palette";
            string result = string.Join(importExportSeparator, ColorPalette.Select(c => ColorHelpers.ColorToHex(c)));
            File.WriteAllText(path, result);
        }
    }
}