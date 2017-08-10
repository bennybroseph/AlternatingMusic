using System;
using System.Threading;

namespace SuperLibrary
{
    public abstract class DefaultThread
    {
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
            while (m_Run && !s_EmergencyAbort)
                UpdateLoop();
#if !DEBUG
            }
            catch (Exception exception)
            {
                ErrorHandling.HandleException(exception);
            }
#endif
        }

        protected abstract void UpdateLoop();
    }
}
