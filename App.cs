using System;
using System.Windows.Forms;

namespace AudioProject
{
    /// <summary>
    /// Global unhandled exception handler
    /// Prevents the app from crashing silently
    /// </summary>
    internal static class App
    {
        public static void RegisterExceptionHandlers()
        {
            // UI thread exceptions
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{e.Exception.Message}",
                    "Audio Compressor — Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            };

            // Background thread exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    MessageBox.Show(
                        $"A critical error occurred:\n\n{ex.Message}",
                        "Audio Compressor — Critical Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            };
        }
    }
}