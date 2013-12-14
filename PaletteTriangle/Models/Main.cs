using System.IO;
using System.Linq;
using System.Reflection;
using Livet;

namespace PaletteTriangle.Models
{
    public class Main : NotificationObject
    {
        static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public void Initialize()
        {
            this.LoadPages();
        }

        public void LoadPages()
        {
            this.Pages = Page.EnumeratePages(Path.Combine(AssemblyDirectory, "Pages")).ToArray();
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
    }
}
