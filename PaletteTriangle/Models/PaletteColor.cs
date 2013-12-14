using System.Windows.Media;
using PaletteTriangle.AdobeSwatchExchange;

namespace PaletteTriangle.Models
{
    public class PaletteColor
    {
        public PaletteColor(Palette parent, ColorEntry entry)
        {
            this.Parent = parent;
            this.Name = entry.Name;
            this.Color = entry.ToColor();
        }

        public Palette Parent { get; private set; }
        public string Name { get; private set; }
        public Color Color { get; private set; }
    }
}
