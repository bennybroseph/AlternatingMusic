using SuperLibrary;
using System;
using System.Windows.Forms;

namespace SuperNetNanny
{
    public static class Program
    {
        private static ReopenThread s_ReopenThread =
            new ReopenThread("DontStopMeNow", "..\\Updater\\DontStopMeNow.exe");

        [STAThread]
        private static void Main()
        {
#if !DEBUG
            try
            {
#endif
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                s_ReopenThread.StartThread();

                var mainForm = new MainForm();
                Application.Run(mainForm);
#if !DEBUG
            }
            catch (Exception exception)
            {
                ErrorHandling.HandleException(exception);
            }
#endif
        }


    }
}
