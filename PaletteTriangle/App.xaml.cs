﻿using System.Windows;
using Livet;

namespace PaletteTriangle
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = this.Dispatcher;
        }
    }
}
