using System;
using System.Linq;
using Livet;
using PaletteTriangle.Models;

namespace PaletteTriangle.ViewModels
{
    public class PageViewModel : ViewModel
    {
        public PageViewModel(MainWindowViewModel parent, Page model)
        {
            this.Parent = parent;
            this.Model = model;
            this.DirectoryUri = new Uri(model.Directory.FullName + "/");
            this.IndexUri = new Uri(this.DirectoryUri, model.IndexPage);
            this.Colors = model.Colors.Select(c => new ColorViewModel(c)).ToArray();
        }

        public MainWindowViewModel Parent { get; private set; }
        public Page Model { get; private set; }

        public string Title
        {
            get
            {
                return this.Model.Title;
            }
        }

        public Uri DirectoryUri { get; private set; }
        public Uri IndexUri { get; private set; }
        public ColorViewModel[] Colors { get; private set; }

        public bool IsCurrent
        {
            get
            {
                return this.Parent.CurrentPage == this;
            }
        }

        public void RaiseIsCurrentChanged()
        {
            this.RaisePropertyChanged(() => this.IsCurrent);
        }
    }
}
