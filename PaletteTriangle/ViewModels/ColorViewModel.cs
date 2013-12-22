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
        public ColorViewModel(PageViewModel parent, VariableColor model)
        {
            this.Parent = parent;
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

        public PageViewModel Parent { get; private set; }
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
            using (var vm = new ColorCanvasViewModel(solid != null
                ? solid.Color
                : (this.Color as LinearGradientBrush).GradientStops.First().Color))
            {

                await this.Messenger.GetResponseAsync(new TransitionMessage(vm, "ShowColorCanvas"));
                if (vm.IsSet)
                    this.Model.Color = new SolidColorBrush(vm.SelectedColor);
            }
        }

        public async void CreateLinearGradient()
        {
            using (var vm = new LinearGradientCanvasViewModel(this.Color, this.Parent.Parent.SelectableColors))
            {
                await this.Messenger.GetResponseAsync(new TransitionMessage(vm, "ShowLinearGradientCanvas"));
                if (vm.IsSet)
                    this.Model.Color = vm.Brush;
            }
        }
    }
}
