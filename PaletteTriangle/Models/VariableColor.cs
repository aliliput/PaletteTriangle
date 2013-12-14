using System;
using System.Windows.Media;
using Livet;

namespace PaletteTriangle.Models
{
    public class VariableColor : NotificationObject
    {
        public VariableColor(Tuple<string, string>[] selectors, string name, string @default)
        {
            this.Selectors = selectors;
            this.Name = name;
            this.Default = @default;
            this.Color = ColorUtil.FromCss(@default);
        }

        public Tuple<string, string>[] Selectors { get; private set; }
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
