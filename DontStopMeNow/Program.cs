using SuperLibrary;
using System;


namespace DontStopMeNow
{
    internal static class Program
    {
        private const string VERSION_PATH =
            "https://docs.google.com/document/d/" +
            "1OkI9zSX4r-0u5AQTtChFz1enFLClXgo4WUiY-0FOOlA/export?format=txt";

        private const string ZIP_PATH =
            "https://drive.google.com/uc?authuser=0&id=0B1p5QkI3_RgGbnBwR2Q0RDVCTWc&export=download";

        private static ReopenThread m_ReopenThread =
            new ReopenThread("SuperNetNanny", "..\\SuperNetNanny\\SuperNetNanny.exe");

        private static UpdaterThread m_UpdaterThread =
            new UpdaterThread(
                m_ReopenThread,
                VERSION_PATH,
                ZIP_PATH,
                "..\\SuperNetNanny\\version.txt",
                "..\\SuperNetNanny\\");

        private static void Main(string[] args)
        {
#if !DEBUG
            try
            {
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
