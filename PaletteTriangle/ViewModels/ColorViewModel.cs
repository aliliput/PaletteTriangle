using System.Windows.Media;
using Livet;
using Livet.EventListeners;
using PaletteTriangle.Models;

namespace PaletteTriangle.ViewModels
{
    public class ColorViewModel : ViewModel
    {
        public ColorViewModel(VariableColor model)
        {
            this.Model = model;
            this.CompositeDisposable.Add(new PropertyChangedEventListener(model)
            {
                {
                    () => model.Color,
                    (sender, e) =>
                    {
                        this.RaisePropertyChanged(() => this.Color);
                        this.RaisePropertyChanged(() => this.CssFormat);
                    }
                }
            });
        }

        public VariableColor Model { get; private set; }

        public string Name
        {
            get
            {
                return this.Model.Name;
            }
        }

        public Color Color
        {
            get
            {
                return this.Model.Color;
            }
        }

        public string CssFormat
        {
            get
            {
                return this.Model.Color.ToCss();
            }
        }
    }
}
