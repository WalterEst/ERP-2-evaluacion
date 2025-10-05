using System;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, args) =>
        {
            MessageBox.Show($"Ocurrió un error inesperado: {args.Exception.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Ocurrió un error crítico: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }
}
