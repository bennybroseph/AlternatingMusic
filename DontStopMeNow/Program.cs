using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace DontStopMeNow
{
    class Program
    {
        private static bool s_Run = true;

        private const string ZIP_PATH =
            "https://drive.google.com/uc?authuser=0&id=0B1p5QkI3_RgGbnBwR2Q0RDVCTWc&export=download";

        private const string VERSION_PATH =
            "https://docs.google.com/document/d/" +
            "1OkI9zSX4r-0u5AQTtChFz1enFLClXgo4WUiY-0FOOlA/export?format=txt";

        private static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif
                var updateThread = new Thread(UpdateThread);
                updateThread.Start();

                MainThread();
#if !DEBUG
            }
            catch (Exception exception)
            {
                var logFile = new StreamWriter("Log\\errorLog.txt");
                logFile.WriteLine(exception.Message);

                CloseOtherProcess();
                s_Run = false;
            }
#endif
        }

        private static void MainThread()
        {
            while (s_Run)
            {
                OpenOtherProcess();

                Thread.Sleep(500);
            }
        }

        private static void UpdateThread()
        {
            while (s_Run)
            {
                using (var clientVersion = new WebClient())
                {
                    clientVersion.DownloadFile(VERSION_PATH, "newVersion.txt");

                    var oldVersionFile = new StreamReader("..\\SuperNetNanny\\version.txt");
                    var newVersionFile = new StreamReader("newVersion.txt");

                    var oldVersion = float.Parse(oldVersionFile.ReadLine());
                    var newVersion = float.Parse(newVersionFile.ReadLine());
                    if (newVersion > oldVersion)
                    {
                        using (var clientZip = new WebClient())
                        {
                            clientZip.DownloadFile(ZIP_PATH, "SuperNetNanny.zip");

                            s_Run = false;
                            CloseOtherProcess();

                            using (var zipArchive = ZipFile.Open("SuperNetNanny.zip", ZipArchiveMode.Read))
                            {
                                foreach (var file in zipArchive.Entries)
                                {
                                    var completeFileName =
                                        Path.Combine(Directory.GetCurrentDirectory(), file.FullName);
                                    var directory = Path.GetDirectoryName(completeFileName);

                                    if (!Directory.Exists(directory))
                                        Directory.CreateDirectory(directory);

                                    if (file.Name != "")
                                        file.ExtractToFile(completeFileName, true);
                                }
                            }
                        }
                        MessageBox.Show(
                            "Oh boy, how wonderful is that!",
                            "Super Net Nanny Updated to v" + newVersion,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        File.Delete("SuperNetNanny.zip");

                        OpenOtherProcess();
                    }
                    newVersionFile.Close();

                    File.Delete("newVersion.txt");
                }

                // Sleep for 5 mins
                Thread.Sleep(5 * 60 * 1000);
            }
        }

        private static void OpenOtherProcess()
        {
            var processCount = Process.GetProcessesByName("SuperNetNanny").Length;
            if (processCount == 0)
                Process.Start("..\\SuperNetNanny\\SuperNetNanny.exe");
        }
        private static void CloseOtherProcess()
        {
            foreach (var process in Process.GetProcessesByName("SuperNetNanny"))
                process.Kill();
        }
    }
}
