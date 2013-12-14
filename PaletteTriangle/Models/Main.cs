using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
            await Task.Run(() => this.Pages = Page.EnumeratePages(Path.Combine(AssemblyDirectory, "Pages")).ToArray());
        }

        private Page[] pages = new Page[0];
        public Page[] Pages
        {
            get
            {
                return this.pages;
            }
            set
            {
                if (!pages.SequenceEqual(value))
                {
                    this.pages = value;
                    this.RaisePropertyChanged();
                }
            }
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
