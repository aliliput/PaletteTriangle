using System;
using System.Collections.Generic;
using System.IO;
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
            this.IndexUri = new Uri(Path.Combine(model.ManifestFile.Directory.FullName, model.IndexPage));
            this.Colors = model.Colors.Select(c => new ColorViewModel(this, c)).ToReadOnlyCollection();
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

        public Uri IndexUri { get; private set; }
        public IReadOnlyCollection<ColorViewModel> Colors { get; private set; }

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.Colors.ForEach(c => c.Dispose());

            base.Dispose(disposing);
        }
    }
}
