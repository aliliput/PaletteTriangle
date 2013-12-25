using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        private void ShowColorsContextMenu(object sender)
        {
            var item = sender as ListBoxItem;
            this.colorsContextMenu.ItemsSource = this.viewModel.SelectableColors;
            this.colorsContextMenu.Placement = PlacementMode.Bottom;
            this.colorsContextMenu.PlacementTarget = item;
            this.colorsContextMenu.Tag = item.DataContext;
            this.colorsContextMenu.IsOpen = true;
        }

        private readonly Dictionary<object, DispatcherTimer> timers = new Dictionary<object, DispatcherTimer>();
        private const double DoubleClickTime = 200; // OS 標準より短め
        private readonly HashSet<object> gotUp = new HashSet<object>();

        private void colorsListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.gotUp.Remove(sender);

            if (this.timers.ContainsKey(sender) && this.timers[sender].IsEnabled)
            {
                this.timers[sender].Stop();
                this.timers.Remove(sender);

                // Double Click
                ((sender as ListBoxItem).DataContext as ColorViewModel).EditColor();
            }
            else
            {
                var timer = new DispatcherTimer();
                timer.Tag = sender;
                timer.Interval = TimeSpan.FromMilliseconds(DoubleClickTime);
                timer.Tick += this.timer_Tick;
                timer.Start();
                this.timers[sender] = timer;
            }
        }

        private void colorsListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.timers.ContainsKey(sender))
            {
                var timer = this.timers[sender] as DispatcherTimer;
                if (timer.IsEnabled)
                {
                    this.gotUp.Add(sender);
                }
                else
                {
                    this.timers.Remove(sender);

                    // Single Click
                    this.ShowColorsContextMenu(sender);
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            timer.Stop();

            if (this.gotUp.Contains(timer.Tag))
            {
                this.gotUp.Remove(timer.Tag);
                this.timers.Remove(timer.Tag);

                // Single Click
                this.ShowColorsContextMenu(timer.Tag);
            }
        }

        private void colorsListBoxItem_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Space)
                this.ShowColorsContextMenu(sender);
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
    }
}
