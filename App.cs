using System;
using System.Windows.Forms;

namespace AudioProject
{
    
    internal static class App
    {
        public static void RegisterExceptionHandlers()
        {
           
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{e.Exception.Message}",
                    "Audio Compressor — Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            };

        
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