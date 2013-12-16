using System.Collections.ObjectModel;
using System.Linq;
using Livet;
using PaletteTriangle.AdobeSwatchExchange;

namespace PaletteTriangle.Models
{
    public class Palette : NotificationObject
    {
        public Palette(Group colorGroup)
        {
            this.Name = colorGroup.Name;
            this.Colors = colorGroup.Colors.Select(c => new PaletteColor(this, c)).ToReadOnlyCollection();
        }

        public string Name { get; private set; }
        public ReadOnlyCollection<PaletteColor> Colors { get; private set; }

        private bool enabled = true;
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }
}
