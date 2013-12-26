using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PaletteTriangle.Models
{
    public class Page
    {
        public Page(FileInfo manifestFile, string title, string indexPage, IEnumerable<VariableColor> colors)
        {
            this.ManifestFile = manifestFile;
            this.Title = title;
            this.IndexPage = indexPage;
            this.Colors = colors.ToReadOnlyCollection();

            this.Colors.ForEach(c => c.PropertyChanged += (sender, e) =>
            {
                if (this.ColorChanged != null)
                    this.ColorChanged(this, new ColorChangedEventArgs((VariableColor)sender));
            });
        }

        public FileInfo ManifestFile { get; private set; }
        public string Title { get; private set; }
        public string IndexPage { get; private set; }
        public IReadOnlyCollection<VariableColor> Colors { get; private set; }

        public event EventHandler<ColorChangedEventArgs> ColorChanged;

        public void SetDefaultColor()
        {
            this.Colors.ForEach(c => c.SetDefaultColor());
        }

        public static IEnumerable<Page> EnumeratePages(string directory)
        {
            XNamespace ns = "http://schemas.azyobuzi.net/PaletteTriangle";
            return new DirectoryInfo(directory).EnumerateFiles("*.xml", SearchOption.AllDirectories)
                .ThroughError(file => Tuple.Create(file, XDocument.Parse(File.ReadAllText(file.FullName)).Element(ns + "page")))
                .ThroughError(x => new Page(
                    x.Item1,
                    x.Item2.Element(ns + "title").Value,
                    x.Item2.Element(ns + "index").Value,
                    x.Item2.Element(ns + "colors").Elements(ns + "color")
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
