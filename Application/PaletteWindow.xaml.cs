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
            colorsListBox.DoubleTapped += async (s, e) => {
                Color selectedColor = ((ColorPaletteItem) colorsListBox.SelectedItem).color;
                SelectColorWindow dialog = new SelectColorWindow(selectedColor);
                Color result = await dialog.ShowDialog<Color>(this);
                ColorPalette[ColorPalette.IndexOf(selectedColor)] = result;
                ReloadPaletteItems();
            };
            ReloadPaletteItems();
            statusText = this.FindControl<TextBlock>("Status");
            medianCutAmountNumeric = this.FindControl<NumericUpDown>("MedianCutColorAmount");
            kMeansAmountNumeric = this.FindControl<NumericUpDown>("KMeansColorAmount");
            kMeansStepsNumeric = this.FindControl<NumericUpDown>("KMeansStepAmount");
            Closing += (s, e) => {
                Hide();
                Showing = false;
                e.Cancel = true;
            };
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
                mainWindow.CurrentBitmap = await Task.Run(() => PaletteGeneration.SortyColors(mainWindow.CurrentBitmap)); 
                //mainWindow.CurrentBitmap = await Task.Run(() => PaletteGeneration.SortColors(mainWindow.CurrentBitmap, amount)); 
                mainWindow.ReloadMainImage();
                statusText.Text = "";
            }
        }
    }
}