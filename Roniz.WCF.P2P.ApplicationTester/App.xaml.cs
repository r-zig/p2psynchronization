using System.Windows;
using System.Windows.Threading;
using Roniz.Diagnostics.Logging;

namespace Roniz.WCF.P2P.ApplicationTester
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            LogManager.GetCurrentClassLogger().Fatal(e.Exception,ExceptionLoggingOptions.Full);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

    }
}
