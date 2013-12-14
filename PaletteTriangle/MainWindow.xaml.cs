using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Livet;
using Livet.EventListeners;
using PaletteTriangle.Models;
using PaletteTriangle.ViewModels;

namespace PaletteTriangle
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainWindowViewModel viewModel;
        readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();
        readonly ContextMenu colorsContextMenu;

        public MainWindow()
        {
            InitializeComponent();
            this.colorsContextMenu = this.FindResource("colorsContextMenu") as ContextMenu;
            this.viewModel = (MainWindowViewModel)this.DataContext;
            this.compositeDisposable.Add(this.viewModel);
            this.compositeDisposable.Add(this.browser);
            this.compositeDisposable.Add(new PropertyChangedEventListener(this.viewModel)
            {
                {
                    () => this.viewModel.Pages,
                    async (_, __) => await this.Dispatcher.InvokeAsync(() =>
                    {
                        var items = this.pagesMenu.Items;
                        Enumerable.Range(0, items.Count - 2).ForEach(i => items.RemoveAt(0));
                        this.viewModel.Pages.Reverse().ForEach(p =>
                        {
                            var item = new MenuItem();
                            item.DataContext = p;
                            item.Header = p.Title;
                            item.Click += (sender, e) => this.viewModel.CurrentPage = (sender as MenuItem).DataContext as PageViewModel;
                            items.Insert(0, item);
                        });
                    })
                },
                {
                    () => this.viewModel.CurrentPage,
                    (_, __) => this.browser.NavigateTo(this.viewModel.CurrentPage.IndexUri.ToString())
                }
            });
            this.compositeDisposable.Add(new CollectionChangedEventListener(this.viewModel.Palettes,
                async (_, __) => await this.Dispatcher.InvokeAsync(() =>
                {
                    var items = this.palettesMenu.Items;
                    Enumerable.Range(0, items.Count - 2).ForEach(i => items.RemoveAt(0));
                    this.viewModel.Palettes.Reverse().ForEach(p =>
                    {
                        var item = new MenuItem();
                        item.DataContext = p;
                        item.Header = p.Name;
                        item.Click += (sender, e) =>
                        {
                            var model = (sender as MenuItem).DataContext as Palette;
                            model.Enabled = !model.Enabled;
                        };
                        items.Insert(0, item);
                    });
                })));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.compositeDisposable.Dispose();
        }

        private void colorsListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            this.colorsContextMenu.ItemsSource = this.viewModel.SelectableColors;
            this.colorsContextMenu.Placement = PlacementMode.Bottom;
            this.colorsContextMenu.PlacementTarget = item;
            this.colorsContextMenu.Tag = item.DataContext;
            this.colorsContextMenu.IsOpen = true;
        }

        private void colorsContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var color = this.colorsContextMenu.Tag as ColorViewModel;
            color.Model.Color = (item.DataContext as PaletteColorViewModel).Color;
        }
    }
}
