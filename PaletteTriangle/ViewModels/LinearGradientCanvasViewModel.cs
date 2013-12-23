using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using Livet;
using Livet.Commands;
using Livet.Messaging;

namespace PaletteTriangle.ViewModels
{
    public class LinearGradientCanvasViewModel : ViewModel
    {
        public LinearGradientCanvasViewModel(Brush brush, IEnumerable<PaletteColorViewModel> selectableColors)
        {
            var linear = brush as LinearGradientBrush;
            if (linear == null)
            {
                var solid = brush as SolidColorBrush;
                linear = new LinearGradientBrush(solid.Color, solid.Color, 90);
            }
            else if (!linear.IsFrozen)
                linear = linear.Clone();

            this.Brush = linear;
            this.ColorList = new DispatcherCollection<Tuple<Color, string>>(
                new ObservableCollection<Tuple<Color, string>>(
                    linear.GradientStops.Select(s => Tuple.Create(s.Color, s.Color.ToCss()))
                ),
                DispatcherHelper.UIDispatcher
            );
            this.SelectableColors = (selectableColors ?? new PaletteColorViewModel[0]).ToReadOnlyCollection();
        }

        public LinearGradientCanvasViewModel()
            : this(new LinearGradientBrush(Colors.White, Colors.White, 90), null)
        { }

        private LinearGradientBrush brush;
        public LinearGradientBrush Brush
        {
            get
            {
                return this.brush;
            }
            private set
            {
                if (this.brush != value)
                {
                    this.brush = value;
                    if (value.CanFreeze) value.Freeze();
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(() => this.CssFormat);
                }
            }
        }

        public string CssFormat
        {
            get
            {
                return this.Brush.ToCss();
            }
        }

        private DispatcherCollection<Tuple<Color, string>> colorList;
        public DispatcherCollection<Tuple<Color, string>> ColorList
        {
            get
            {
                return this.colorList;
            }
            private set
            {
                if (this.colorList != value)
                {
                    if (this.colorList != null)
                        this.colorList.CollectionChanged -= this.ColorList_CollectionChanged;
                    this.colorList = value;
                    value.CollectionChanged += this.ColorList_CollectionChanged;
                    this.RaisePropertyChanged();
                }
            }
        }

        private void ColorList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OkCommand.RaiseCanExecuteChanged();
            var n = 1.0 / (this.ColorList.Count - 1);
            this.Brush = new LinearGradientBrush(new GradientStopCollection(
                this.ColorList.Select((c, i) => new GradientStop(c.Item1, n * i))), 90);
        }

        public Tuple<Color, string> selectedColor;
        public Tuple<Color, string> SelectedColor
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
                    this.RemoveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public IReadOnlyCollection<PaletteColorViewModel> SelectableColors { get; private set; }

        public void AddColor(Color color)
        {
            this.ColorList.Add(Tuple.Create(color, color.ToCss()));
        }

        public async void AddColorFromCanvas()
        {
            using (var vm = new ColorCanvasViewModel())
            {
                await this.Messenger.GetResponseAsync(new TransitionMessage(vm, "ShowColorCanvas"));
                if (vm.IsSet)
                    this.AddColor(vm.SelectedColor);
            }
        }

        private ViewModelCommand removeCommand;
        public ViewModelCommand RemoveCommand
        {
            get
            {
                return this.removeCommand = this.removeCommand ?? new ViewModelCommand(
                    () => this.ColorList.Remove(this.SelectedColor),
                    () => this.SelectedColor != null
                );
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

        private ViewModelCommand okCommand;
        public ViewModelCommand OkCommand
        {
            get
            {
                return this.okCommand = this.okCommand ?? new ViewModelCommand(
                    async () =>
                    {
                        this.IsSet = true;
                        await this.Messenger.RaiseAsync(new InteractionMessage("Close"));
                    },
                    () => this.ColorList.Count >= 2
                );
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.ColorList != null)
                this.ColorList.CollectionChanged -= this.ColorList_CollectionChanged;

            base.Dispose(disposing);
        }
    }
}
