using System.Diagnostics;
using System.Threading;

namespace SuperLibrary
{
    public class ReopenThread : DefaultThread
    {
        private string m_ProcessName;
        private string m_ExecutablePath;

        private bool m_AllowReopen = true;

        public bool allowReopen { get => m_AllowReopen; set => m_AllowReopen = value; }

        public ReopenThread(string processName, string executablePath, int loopDelay = 500)
        {
            m_ProcessName = processName;
            m_ExecutablePath = executablePath;

            m_LoopDelay = loopDelay;
        }

        public void CloseProcesses()
        {
            var processes = Process.GetProcessesByName(m_ProcessName);
            foreach (var process in processes)
                process.Kill();
        }

        protected override void UpdateLoop()
        {
            if (m_AllowReopen)
            {
                var processCount = Process.GetProcessesByName(m_ProcessName).Length;
                if (processCount == 0)
                    Process.Start(m_ExecutablePath);
            }

            Thread.Sleep(m_LoopDelay);
        }
    }
}
