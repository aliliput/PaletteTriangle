using System.Linq;
using Livet;
using Livet.EventListeners;
using PaletteTriangle.Models;

namespace PaletteTriangle.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public MainWindowViewModel()
        {
            this.Model = new Main();
            this.CompositeDisposable.Add(new PropertyChangedEventListener(this.Model)
            {
                {
                    () => this.Model.Pages,
                    (sender, e) =>
                    {
                        var newPages = this.Model.Pages.Select(p => new PageViewModel(this, p)).ToArray();
                        var newCurrentPage = this.CurrentPage != null
                            ? newPages.FirstOrDefault(p => p.DirectoryUri == this.CurrentPage.DirectoryUri)
                            : null;
                        this.Pages.ForEach(p => p.Dispose());
                        this.Pages = newPages;
                        this.CurrentPage = newCurrentPage;
                    }
                }
            });
        }

        public Main Model { get; private set; }

        public void Initialize()
        {
            this.Model.Initialize();
        }

        private PageViewModel[] pages = new PageViewModel[0];
        public PageViewModel[] Pages
        {
            get
            {
                return this.pages;
            }
            set
            {
                if (this.pages != value)
                {
                    this.pages = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private PageViewModel currentPage;
        public PageViewModel CurrentPage
        {
            get
            {
                return this.currentPage;
            }
            set
            {
                if (this.currentPage != value)
                {
                    var old = this.currentPage;
                    this.currentPage = value;
                    this.RaisePropertyChanged();
                    if (old != null) old.RaiseIsCurrentChanged();
                    value.RaiseIsCurrentChanged();
                }
            }
        }

        public void ReloadPages()
        {
            this.Model.LoadPages();
        }
    }
}
