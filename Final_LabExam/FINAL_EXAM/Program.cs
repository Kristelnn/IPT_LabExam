using System;
using System.Windows.Forms;

namespace ClassmatesRPG
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ApplicationConfiguration.Initialize();

                // Set default exception handling
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical Error: {ex.Message}\n\nPlease ensure all required files are present and try again.",
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"An error occurred: {e.Exception.Message}\n\nThe application will continue running.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show($"A critical error occurred: {ex?.Message ?? "Unknown Error"}\n\nThe application needs to close.",
                "Critical Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}