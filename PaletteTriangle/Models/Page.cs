using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PaletteTriangle.Models
{
    public class Page
    {
        public Page(DirectoryInfo directory, string title, string indexPage, VariableColor[] colors)
        {
            this.Directory = directory;
            this.Title = title;
            this.IndexPage = indexPage;
            this.Colors = colors;
        }

        public DirectoryInfo Directory { get; private set; }
        public string Title { get; private set; }
        public string IndexPage { get; private set; }
        public VariableColor[] Colors { get; private set; }

        public static IEnumerable<Page> EnumeratePages(string directory)
        {
            return new DirectoryInfo(directory).EnumerateDirectories()
                .Select(d => new { Directory = d, Manifest = Path.Combine(d.FullName, "manifest.xml") })
                .Where(x => File.Exists(x.Manifest))
                .Select(x => new { x.Directory, Manifest = XDocument.Parse(File.ReadAllText(x.Manifest)).Element("page") })
                .Select(x => new Page(
                    x.Directory,
                    x.Manifest.Element("title").Value,
                    x.Manifest.Element("index").Value,
                    x.Manifest.Element("colors").Elements("color")
                        .Select(c => new VariableColor(
                            c.Elements("selector")
                                .Select(s => Tuple.Create(s.Attribute("selector").Value, s.Attribute("property").Value))
                                .ToArray(),
                            c.Attribute("name").Value,
                            c.Attribute("default").Value,
                            c.Attribute("template") != null ? c.Attribute("template").Value : null
                        ))
                        .ToArray()
                ));
        }
    }
}
