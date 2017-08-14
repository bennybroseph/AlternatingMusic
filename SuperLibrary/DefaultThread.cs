
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace SuperLibrary
{
    public abstract class DefaultThread
    {
        public static readonly string APP_PATH =
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private static bool s_EmergencyAbort;

        protected Thread m_Thread;

        protected bool m_Run = true;

        protected int m_LoopDelay;

        public static bool _emergencyAbort { get => s_EmergencyAbort; set => s_EmergencyAbort = value; }

        public bool run { get => m_Run; set => m_Run = value; }
        public Thread thread { get => m_Thread; }

        public Thread StartThread()
        {
            m_Thread = new Thread(CatchFunction);
            m_Thread.Start();

            return m_Thread;
        }

        private void CatchFunction()
        {
#if !DEBUG
            try
            {
#endif
            OnStartLoop();

            while (m_Run && !s_EmergencyAbort)
                UpdateLoop();

            OnExitLoop();
#if !DEBUG
            }
            catch (Exception exception)
            {
                ErrorHandling.HandleException(exception);
            }
#endif
        }

        protected virtual void OnStartLoop() { }

        protected abstract void UpdateLoop();

        protected virtual void OnExitLoop() { }
    }
}
