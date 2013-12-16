using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PaletteTriangle.Models
{
    public class Page
    {
        public Page(DirectoryInfo directory, string title, string indexPage, IEnumerable<VariableColor> colors)
        {
            this.Directory = directory;
            this.Title = title;
            this.IndexPage = indexPage;
            this.Colors = colors.ToReadOnlyCollection();

            this.Colors.ForEach(c => c.PropertyChanged += (sender, e) =>
            {
                if (this.ColorChanged != null)
                    this.ColorChanged(this, new ColorChangedEventArgs((VariableColor)sender));
            });
        }

        public DirectoryInfo Directory { get; private set; }
        public string Title { get; private set; }
        public string IndexPage { get; private set; }
        public ReadOnlyCollection<VariableColor> Colors { get; private set; }

        public event EventHandler<ColorChangedEventArgs> ColorChanged;

        public void SetDefaultColor()
        {
            this.Colors.ForEach(c => c.Color = ColorUtil.FromCss(c.Default));
        }

        public static IEnumerable<Page> EnumeratePages(string directory)
        {
            XNamespace ns = "http://schemas.azyobuzi.net/PaletteTriangle";
            return new DirectoryInfo(directory).EnumerateDirectories()
                .Select(d => new { Directory = d, Manifest = Path.Combine(d.FullName, "manifest.xml") })
                .Where(x => File.Exists(x.Manifest))
                .Select(x => new { x.Directory, Manifest = XDocument.Parse(File.ReadAllText(x.Manifest)).Element(ns + "page") })
                .Select(x => new Page(
                    x.Directory,
                    x.Manifest.Element(ns + "title").Value,
                    x.Manifest.Element(ns + "index").Value,
                    x.Manifest.Element(ns + "colors").Elements(ns + "color")
                        .Select(c => new VariableColor(
                            c.Elements(ns + "selector").Select(s => new Selector(
                                s.Attribute("selector").Value,
                                s.Attribute("property").Value,
                                (string)s.Attribute("template")
                            )),
                            c.Attribute("name").Value,
                            c.Attribute("default").Value
                        ))
                ));
        }
    }

    public class ColorChangedEventArgs : EventArgs
    {
        public ColorChangedEventArgs(VariableColor target)
        {
            this.Target = target;
        }

        public VariableColor Target { get; private set; }
    }
}
