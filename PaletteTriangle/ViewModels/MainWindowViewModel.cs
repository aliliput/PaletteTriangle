using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Livet;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
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
                        var newPages = this.Model.Pages.Select(p => new PageViewModel(this, p)).ToReadOnlyCollection();
                        var newCurrentPage = this.CurrentPage != null
                            ? newPages.FirstOrDefault(p => p.Model.ManifestFile.FullName.Equals(this.CurrentPage.Model.ManifestFile.FullName, StringComparison.InvariantCultureIgnoreCase))
                            : null;
                        this.Pages.ForEach(p => p.Dispose());
                        this.Pages = newPages;
                        this.CurrentPage = newCurrentPage;
                    }
                },
                {
                    () => this.Model.EnabledPalettes,
                    (sender, e) => this.SelectableColors = this.Model.EnabledPalettes
                        .SelectMany(p => p.Colors.Select(c => new PaletteColorViewModel(c)))
                        .ToReadOnlyCollection()
                }
            });
            this.CompositeDisposable.Add(new EventListener<EventHandler<CreatedScriptToRunEventArgs>>(
                h => this.Model.CreatedScriptToRun += h,
                h => this.Model.CreatedScriptToRun -= h,
                async (sender, e) => await this.Messenger.RaiseAsync(new GenericInteractionMessage<string>(e.Script, "RunScript"))
            ));
        }

        public Main Model { get; private set; }

        public void Initialize()
        {
            this.Model.Initialize();
        }

        private IReadOnlyCollection<PageViewModel> pages = new PageViewModel[0];
        public IReadOnlyCollection<PageViewModel> Pages
        {
            get
            {
                return this.pages;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();

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
                    this.Model.CurrentPage = value.Model;
                    this.RaisePropertyChanged();
                    if (old != null) old.RaiseIsCurrentChanged();
                    value.RaiseIsCurrentChanged();
                }
            }
        }

        public async void ReloadPages()
        {
            await this.Model.LoadPages();
        }

        public void LoadedNewPage()
        {
            this.Model.ApplyAllColors();
        }

        public ObservableCollection<Palette> Palettes
        {
            get
            {
                return this.Model.Palettes;
            }
        }

        private IReadOnlyCollection<PaletteColorViewModel> selectableColors = new PaletteColorViewModel[0];
        public IReadOnlyCollection<PaletteColorViewModel> SelectableColors
        {
            get
            {
                return this.selectableColors;
            }
            private set
            {
                if (this.selectableColors != value)
                {
                    this.selectableColors.ForEach(c => c.Dispose());
                    this.selectableColors = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public async void AddPalettes()
        {
            var msg = await this.Messenger.GetResponseAsync(new OpeningFileSelectionMessage("OpenPaletteFiles")
            {
                Title = "パレット追加",
                Filter = "Adobe Swatch Exchange Format|*.ase|すべてのファイル|*.*",
                MultiSelect = true
            });

            var result = await Task.WhenAll(msg.Response.Select(this.Model.AddPalette));

            if (result.Contains(false))
                await this.Messenger.RaiseAsync(new InformationMessage("パレットの読み込みに失敗しました。", "エラー", MessageBoxImage.Error, "MessageBox"));
        }

        private ColorViewModel selectedColor;
        public ColorViewModel SelectedColor
        {
            get
            {
                return this.selectedColor;
            }
            set
            {
                if (this.selectedColor != value)
                {
                    this.selectedColor = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }
}
