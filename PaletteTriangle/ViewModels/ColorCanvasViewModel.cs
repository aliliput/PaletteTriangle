using System.Windows.Media;
using Livet;

namespace PaletteTriangle.ViewModels
{
    public class ColorCanvasViewModel : ViewModel
    {
        public ColorCanvasViewModel() : this(Colors.White) { }

        public ColorCanvasViewModel(Color color)
        {
            this.SelectedColor = color;
        }

        private Color selectedColor;
        public Color SelectedColor
        {
            get
            {
                return this.selectedColor;
            }
            set
            {
                if (this.selectedColor != value)
                {
                    this.selectedColor = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private bool isSet = false;
        public bool IsSet
        {
            get
            {
                return this.isSet;
            }
            set
            {
                if (this.isSet != value)
                {
                    this.isSet = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }
}
