using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Livet;

namespace PaletteTriangle.Models
{
    public class VariableColor : NotificationObject
    {
        public VariableColor(IEnumerable<Selector> selectors, string name, string @default)
        {
            this.Selectors = selectors.ToReadOnlyCollection();
            this.Name = name;
            this.Default = @default;
            this.Color = ColorUtil.FromCss(@default);
        }

        public ReadOnlyCollection<Selector> Selectors { get; private set; }
        public string Name { get; private set; }
        public string Default { get; private set; }

        private Color color;
        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                if (this.color != value)
                {
                    this.color = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }
}
