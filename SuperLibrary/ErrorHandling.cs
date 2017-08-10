using System;
using System.IO;
using System.Windows.Forms;

namespace SuperLibrary
{
    public static class ErrorHandling
    {
        public static void HandleException(Exception exception)
        {
            MessageBox.Show(
                exception.Message + "\nCheck the errorLog.txt for more information",
                "An error occured",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            var logFile = new StreamWriter("Logs\\errorLog.txt");
            logFile.WriteLine(exception.Message);

            DefaultThread._emergencyAbort = true;

            throw exception;
        }
    }
}
