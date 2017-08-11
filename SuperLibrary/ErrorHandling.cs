using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SuperLibrary
{
    public static class ErrorHandling
    {
        public static void HandleException(Exception exception)
        {
            var completeLogsPath = Path.Combine(DefaultThread.APP_PATH, "Logs");
            if (!Directory.Exists(completeLogsPath))
                Directory.CreateDirectory(completeLogsPath);

            using (var logFile = new StreamWriter("Logs\\errorLog.txt", true))
            {
                logFile.WriteLine("[" + DateTime.Now + "]");
                logFile.WriteLine(exception.Source);
                logFile.WriteLine(exception.StackTrace);
                logFile.WriteLine(exception.Message);
                logFile.WriteLine();
            }

            var timer = new Timer { Interval = 5000 };
            timer.Tick += OnTimer;
            timer.Start();

            MessageBox.Show(
                exception.Message + "\nCheck the errorLog.txt for more information\n" + completeLogsPath,
                exception.Source + " encountered an error!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            DefaultThread._emergencyAbort = true;

            throw exception;
        }

        private static void OnTimer(object sender, EventArgs eventArgs)
        {
            SendKeys.SendWait("{ENTER}");
        }
    }
}
