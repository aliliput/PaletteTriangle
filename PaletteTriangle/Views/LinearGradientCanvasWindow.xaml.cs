using System.Windows;
using System.Windows.Controls;
using PaletteTriangle.ViewModels;

namespace PaletteTriangle.Views
{
    /// <summary>
    /// LinearGradientCanvasWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LinearGradientCanvasWindow : Window
    {
        public LinearGradientCanvasWindow()
        {
            InitializeComponent();
        }

        private void addButton_Opened(object sender, RoutedEventArgs e)
        {
            this.selectableColorsList.SelectedItem = null;
        }

        private void selectableColorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.selectableColorsList.SelectedItem != null)
            {
                (this.DataContext as LinearGradientCanvasViewModel)
                    .AddColor((this.selectableColorsList.SelectedItem as PaletteColorViewModel).Color);
                this.addButton.IsOpen = false;
            }
        }
    }
}
