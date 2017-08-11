using SuperLibrary;
using System;


namespace DontStopMeNow
{
    internal static class Program
    {
        private const string VERSION_URL =
            "https://docs.google.com/document/d/" +
            "1OkI9zSX4r-0u5AQTtChFz1enFLClXgo4WUiY-0FOOlA/export?format=txt";

        private const string ZIP_URL =
            "https://dl.dropboxusercontent.com/s/enuddmd2m1eopgs/SuperNetNanny.zip?dl=0";

        private static ReopenThread m_ReopenThread =
            new ReopenThread("SuperNetNanny", "..\\SuperNetNanny\\SuperNetNanny.exe");

        private static UpdaterThread m_UpdaterThread =
            new UpdaterThread(
                m_ReopenThread,
                VERSION_URL,
                ZIP_URL,
                "..\\SuperNetNanny\\version.txt",
                "..\\SuperNetNanny\\");

        private static void Main(string[] args)
        {
#if !DEBUG
            try
            {
                ConsoleCommands.HideConsole();
#endif
            m_ReopenThread.StartThread();
            m_UpdaterThread.StartThread();
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
