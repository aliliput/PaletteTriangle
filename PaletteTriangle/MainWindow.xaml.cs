using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Livet.EventListeners;
using PaletteTriangle.ViewModels;

namespace PaletteTriangle
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainWindowViewModel viewModel;
        readonly PropertyChangedEventListener viewModelPropertyChangedEventListener;

        public MainWindow()
        {
            InitializeComponent();
            this.viewModel = (MainWindowViewModel)this.DataContext;
            this.viewModelPropertyChangedEventListener = new PropertyChangedEventListener(this.viewModel)
            {
                {
                    () => this.viewModel.Pages,
                    (_, __) =>
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
                    }
                },
                {
                    () => this.viewModel.CurrentPage,
                    (_, __) => this.browser.Navigate(this.viewModel.CurrentPage.IndexUri)
                }
            };
        }

        private void browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (!e.Uri.AbsoluteUri.StartsWith(this.viewModel.CurrentPage.DirectoryUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
                e.Cancel = true;
        }
    }
}
