using System.Windows.Media;
using Livet;
using PaletteTriangle.Models;

namespace PaletteTriangle.ViewModels
{
    public class PaletteColorViewModel : ViewModel
    {
        public PaletteColorViewModel(PaletteColor model)
        {
            this.Model = model;
        }

        public PaletteColor Model { get; private set; }

        public string Name
        {
            get
            {
                return this.Model.Name;
            }
        }

        public string GroupName
        {
            get
            {
                return this.Model.Parent.Name;
            }
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0} ({1})", this.Name, this.GroupName).Trim();
            }
        }

        public Color Color
        {
            get
            {
                return this.Model.Color;
            }
        }
    }
}
