using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;

namespace WSL_Manager
{
    public partial class App : Application
    {
        private Window? _window;
        private static readonly string LogFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WSL Manager", "logs.txt");

        public App()
        {
            InitializeComponent();

            // Handlers globaux pour attraper les exceptions et les écrire dans un fichier de log
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            this.UnhandledException += App_UnhandledException;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                _window = new MainWindow();
                _window.Activate();
                LogInfo("Application lancée avec succès.");
            }
            catch (Exception ex)
            {
                LogError("Exception dans OnLaunched", ex);
                throw;
            }
        }

        private void App_UnhandledException(object? sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LogError("UnhandledException UI thread", e.Exception);
        }

        private void CurrentDomain_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            LogError("CurrentDomain.UnhandledException", e.ExceptionObject as Exception);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogError("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved();
        }

        private static void EnsureLogDirectory()
        {
            try
            {
                var dir = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
            }
            catch { /* Échec de la création de dossier, on ignore pour ne pas casser l'app */ }
        }

        private static void LogInfo(string message)
        {
            try
            {
                EnsureLogDirectory();
                File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] {message}{Environment.NewLine}");
            }
            catch { }
        }

        private static void LogError(string message, Exception? ex)
        {
            try
            {
                EnsureLogDirectory();
                File.AppendAllText(LogFilePath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {message} - {ex?.Message}{Environment.NewLine}{ex?.StackTrace}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
