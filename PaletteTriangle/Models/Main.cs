using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Livet;
using PaletteTriangle.AdobeSwatchExchange;

namespace PaletteTriangle.Models
{
    public class Main : NotificationObject
    {
        static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public async void Initialize()
        {
            await this.LoadPages();
        }

        public async Task LoadPages()
        {
            await Task.Run(() => this.Pages = Page.EnumeratePages(Path.Combine(AssemblyDirectory, "Pages")).ToReadOnlyCollection());
        }

        private ReadOnlyCollection<Page> pages = Enumerable.Empty<Page>().ToReadOnlyCollection();
        public ReadOnlyCollection<Page> Pages
        {
            get
            {
                return this.pages;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();

                if (!pages.SequenceEqual(value))
                {
                    this.pages.ForEach(p => p.ColorChanged -= this.Pages_ColorChanged);
                    this.pages = value;
                    value.ForEach(p => p.ColorChanged += this.Pages_ColorChanged);
                    this.RaisePropertyChanged();
                }
            }
        }

        private void Pages_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (sender == this.CurrentPage)
                this.ApplyColor(e.Target);
        }

        private Page currentPage;
        public Page CurrentPage
        {
            get
            {
                return this.currentPage;
            }
            set
            {
                if (currentPage != value)
                {
                    this.currentPage = value;
                    value.SetDefaultColor();
                    this.RaisePropertyChanged();
                }
            }
        }

        public event EventHandler<CreatedScriptToRunEventArgs> CreatedScriptToRun;

        private void ApplyColor(VariableColor color)
        {
            if (this.CreatedScriptToRun != null)
                color.Selectors.ForEach(s =>
                {
                    var script = string.Format(
                        @"(function() {{
                            var elements = document.querySelectorAll(""{0}"");
                            for (var i = 0; i < elements.length; i++) {{
                                elements[i].style.{1} = ""{2}"";
                            }}
                        }})();",
                        EscapeJsStr(s.Query),
                        Regex.Replace(s.Property, @"\-(.)", m => m.Groups[1].Value.ToUpper()),
                        EscapeJsStr(string.Format(string.IsNullOrEmpty(s.Template) ? "{0}" : s.Template, color.Color.ToCss()))
                    );
                    this.CreatedScriptToRun(this, new CreatedScriptToRunEventArgs(script));
                });
        }

        public void ApplyAllColors()
        {
            if (this.CurrentPage != null)
                this.CurrentPage.Colors.ForEach(this.ApplyColor);
        }

        private static string EscapeJsStr(string source)
        {
            return source.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");
        }

        private ObservableCollection<Palette> palettes = new ObservableCollection<Palette>();
        public ObservableCollection<Palette> Palettes
        {
            get
            {
                return this.palettes;
            }
        }

        public async Task<bool> AddPalette(string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    AseFile.FromFile(fileName).Groups.ForEach(g => this.Palettes.Add(new Palette(g)));
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
