using System.Linq;
using System.Windows.Media;
using Livet;
using Livet.EventListeners;
using Livet.Messaging;
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

        public Brush Color
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

        public void SetDefaultColor()
        {
            this.Model.SetDefaultColor();
        }

        public async void EditColor()
        {
            var solid = this.Color as SolidColorBrush;
            var vm = new ColorCanvasViewModel(solid != null
                ? solid.Color
                : (this.Color as LinearGradientBrush).GradientStops.First().Color
            );
            
            await this.Messenger.GetResponseAsync(new TransitionMessage(vm, "ShowColorCanvas"));

            if (vm.IsSet)
            {
                this.Model.Color = new SolidColorBrush(vm.SelectedColor);
            }
        }
    }
}
