using System;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Net;

namespace SuperLibrary
{
    public class UpdaterThread : DefaultThread
    {
        private const string NEW_VERSION_PATH = "newVersion.txt";
        private const string ZIP_DOWNLOAD_PATH = "NewApplication.zip";

        private ReopenThread m_ReopenThread;

        private string m_VersionUrl;
        private string m_ZipUrl;

        private string m_OldVersionPath;
        private string m_ExtractionPath;

        private object m_ProgressLock = new object();

        public UpdaterThread(
            ReopenThread reopenThread,
            string versionUrl,
            string zipUrl,
            string oldVersionPath,
            string extractionPath)
        {
            m_ReopenThread = reopenThread;

            m_VersionUrl = versionUrl;
            m_ZipUrl = zipUrl;

            m_OldVersionPath = Path.Combine(APP_PATH, oldVersionPath);

            m_ExtractionPath = Path.Combine(APP_PATH, extractionPath);
        }

        protected override void UpdateLoop()
        {
            using (var clientVersion = new WebClient())
            {
                var completeNewVersionPath = Path.Combine(APP_PATH, NEW_VERSION_PATH);
                clientVersion.DownloadFile(m_VersionUrl, completeNewVersionPath);

                var oldVersionFile = new StreamReader(m_OldVersionPath);
                var newVersionFile = new StreamReader(completeNewVersionPath);

                var oldVersion = float.Parse(oldVersionFile.ReadLine());
                var newVersion = float.Parse(newVersionFile.ReadLine());
                if (newVersion > oldVersion)
                {
                    oldVersionFile.Close();
                    newVersionFile.Close();

                    var completeZipPath = Path.Combine(APP_PATH, ZIP_DOWNLOAD_PATH);
                    using (var clientZip = new WebClient())
                    {
                        ConsoleCommands.ShowConsole();

                        Console.WriteLine("Downloading new version...");

                        clientZip.DownloadProgressChanged += OnDownloadProgressChanged;
                        var task = clientZip.DownloadFileTaskAsync(m_ZipUrl, completeZipPath);
                        while (!task.IsCompleted)
                            Thread.Sleep(1000);

                        Console.WriteLine("Download Complete!\n");

                        Console.WriteLine("Closing other process...");

                        m_ReopenThread.allowReopen = false;

                        Thread.Sleep(1000);
                        m_ReopenThread.CloseProcesses();

                        Console.WriteLine("Done!\n");

                        Console.WriteLine("Extracting Files...");
                        using (var zipArchive = ZipFile.Open(completeZipPath, ZipArchiveMode.Read))
                        {
                            foreach (var file in zipArchive.Entries)
                            {
                                var completeFileName = Path.Combine(m_ExtractionPath, file.FullName);
                                var directory = Path.GetDirectoryName(completeFileName);

                                if (!Directory.Exists(directory))
                                    Directory.CreateDirectory(directory);

                                if (file.Name != "")
                                    file.ExtractToFile(completeFileName, true);

                                Console.WriteLine(
                                    "Writing " + file.Name);
                            }
                        }
                    }
                    Console.WriteLine("Update Complete!");

                    File.Delete(completeZipPath);

                    Thread.Sleep(1000 * 5);
                    ConsoleCommands.HideConsole();

                    m_ReopenThread.allowReopen = true;
                }
                oldVersionFile.Close();
                newVersionFile.Close();

                File.Delete(completeNewVersionPath);
            }

            // Sleep for 5 mins
            Thread.Sleep(5 * 60 * 1000);
        }

        private void OnDownloadProgressChanged(
            object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            lock (m_ProgressLock)
            {
                Console.CursorLeft = 0;
                Console.Write("[");

                var downloadProgress = (float)downloadProgressChangedEventArgs.ProgressPercentage;
                var maxProgressBar = 15f;
                for (var i = 0; i < maxProgressBar; i++)
                    Console.Write(downloadProgress / 100f >= (i + 1) / maxProgressBar ? ((char)219).ToString() : "-");

                Console.Write("] " + downloadProgressChangedEventArgs.ProgressPercentage + "%\t");
            }
        }
    }
}
