using System;
using Xilium.CefGlue;

namespace PaletteTriangle
{
    static class EntryPoint
    {
        [STAThread]
        static int Main(string[] args)
        {
            CefRuntime.Load();

            var mainArgs = new CefMainArgs(args);
            var cefApp = new CefAppImpl();

            var exitCode = CefRuntime.ExecuteProcess(mainArgs, cefApp);
            if (exitCode != -1) return exitCode;

            var cefSettings = new CefSettings
            {
#if DEBUG
                SingleProcess = true,
#else
                SingleProcess = false,
#endif
                MultiThreadedMessageLoop = true,
                LogSeverity = CefLogSeverity.Default,
                LogFile = "cef.log",
            };

            CefRuntime.Initialize(mainArgs, cefSettings, cefApp);

            var app = new App();
            app.InitializeComponent();
            app.Run();

            CefRuntime.Shutdown();
            return 0;
        }
    }
}
