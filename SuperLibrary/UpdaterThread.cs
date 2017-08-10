using System;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Net;

namespace SuperLibrary
{
    public class UpdaterThread : DefaultThread
    {
        private ReopenThread m_ReopenThread;

        private string m_VersionUrl;
        private string m_ZipUrl;

        private string m_OldVersionPath;

        private string m_ExtractionPath;

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

            m_OldVersionPath = oldVersionPath;

            m_ExtractionPath = extractionPath;
        }

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        protected override void UpdateLoop()
        {
            using (var clientVersion = new WebClient())
            {
                clientVersion.DownloadFile(m_VersionUrl, "newVersion.txt");

                var oldVersionFile = new StreamReader(m_OldVersionPath);
                var newVersionFile = new StreamReader("newVersion.txt");

                var oldVersion = float.Parse(oldVersionFile.ReadLine());
                var newVersion = float.Parse(newVersionFile.ReadLine());
                if (newVersion > oldVersion)
                {
                    using (var clientZip = new WebClient())
                    {
                        AllocConsole();

                        Console.WriteLine("Downloading new version...");

                        clientZip.DownloadProgressChanged += OnDownloadProgressChanged;
                        clientZip.DownloadFileAsync(new Uri(m_ZipUrl), "NewApplication.zip");

                        m_ReopenThread.allowReopen = false;
                        m_ReopenThread.CloseProcesses();

                        using (var zipArchive = ZipFile.Open("NewApplication.zip", ZipArchiveMode.Read))
                        {
                            foreach (var file in zipArchive.Entries)
                            {
                                var completeFileName = Path.Combine(m_ExtractionPath, file.FullName);
                                var directory = Path.GetDirectoryName(completeFileName);

                                if (!Directory.Exists(directory))
                                    Directory.CreateDirectory(directory);

                                if (file.Name != "")
                                    file.ExtractToFile(completeFileName, true);
                            }
                        }
                    }
                    Console.WriteLine("Update Complete!");

                    File.Delete("SuperNetNanny.zip");

                    Thread.Sleep(1000 * 5);

                    FreeConsole();

                    m_ReopenThread.allowReopen = true;
                }
                newVersionFile.Close();

                File.Delete("newVersion.txt");
            }

            // Sleep for 5 mins
            Thread.Sleep(5 * 60 * 1000);
        }

        private void OnDownloadProgressChanged(
            object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            Console.WriteLine(downloadProgressChangedEventArgs.ProgressPercentage);
        }
    }
}
