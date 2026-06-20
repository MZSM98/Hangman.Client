using Hangman.Client.Localization;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Hangman.Client
{
    public partial class App : Application
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureLogging();
            RegisterGlobalExceptionHandlers();
            LanguageManager.RestoreLanguage();
            Log.Info("Hangman.Client started.");
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info("Hangman.Client closed.");
            base.OnExit(e);
        }

        private static void ConfigureLogging()
        {
            string configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "log4net.config");

            FileInfo configFile = new FileInfo(configPath);

            if (configFile.Exists)
            {
                XmlConfigurator.ConfigureAndWatch(configFile);
            }
        }

        private void RegisterGlobalExceptionHandlers()
        {
            DispatcherUnhandledException += (sender, args) =>
            {
                Log.Error("Unhandled WPF dispatcher exception.", args.Exception);

                MessageBox.Show(
                    "Ocurrió un error inesperado en la aplicación.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception exception = args.ExceptionObject as Exception;

                if (exception != null)
                {
                    Log.Error("Unhandled application domain exception.", exception);
                }
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Error("Unobserved task exception.", args.Exception);
                args.SetObserved();
            };
        }
    }
}
