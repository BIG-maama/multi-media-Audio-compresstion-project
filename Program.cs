using System;
using System.Windows.Forms;

namespace AudioProject
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            App.RegisterExceptionHandlers();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}