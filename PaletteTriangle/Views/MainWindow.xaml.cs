using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Livet;
using Livet.EventListeners;
using Livet.Messaging;
using PaletteTriangle.Models;
using PaletteTriangle.ViewModels;
using Xilium.CefGlue;
using Xilium.CefGlue.WPF;
using System.Threading.Tasks;

namespace PaletteTriangle.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel;
        private readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();
        private readonly ContextMenu colorsContextMenu;
        private DispatcherTimer timer;

        private CefBrowser cefBrowser;
        private bool isLoading = false;

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
            this.compositeDisposable.Add(new MessageListener(
                this.viewModel.Messenger,
                "RunScript",
                async m => await this.RunScript((m as GenericInteractionMessage<string>).Value)
            ));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewModel.Initialize();

            this.timer = new DispatcherTimer();
            this.timer.Tick += (_, __) =>
            {
                if (this.cefBrowser == null)
                    this.cefBrowser = (CefBrowser)typeof(WpfCefBrowser).InvokeMember(
                        "_browser",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                        null,
                        this.browser,
                        null
                    );

                if (this.cefBrowser != null)
                {
                    if (!this.isLoading && this.cefBrowser.IsLoading && this.cefBrowser.HasDocument)
                    {
                        this.isLoading = true;
                        this.viewModel.LoadedNewPage();
                    }
                    else
                    {
                        this.isLoading = this.cefBrowser.IsLoading;
                    }
                }
            };
            this.timer.Interval = TimeSpan.FromSeconds(0.1);
            this.timer.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.timer != null) this.timer.Stop();
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
            color.Model.Color = new SolidColorBrush((item.DataContext as PaletteColorViewModel).Color);
        }

        private async Task RunScript(string script)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.cefBrowser == null || !this.cefBrowser.HasDocument) return;

                var frame = this.cefBrowser.GetMainFrame();
                frame.ExecuteJavaScript(script, frame.Url, 0);
            });
        }

        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((sender as MenuItem).DataContext as ColorViewModel).CssFormat);
        }
    }
}
