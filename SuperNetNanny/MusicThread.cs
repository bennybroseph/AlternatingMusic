using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AudioSwitcher.AudioApi.CoreAudio;
using SuperLibrary;

namespace SuperNetNanny
{
    public class MusicThread : DefaultThread
    {
        private const int MAX_VOLUME = 30;

        private const string PREFERENCES_PATH = "preferences.txt";

        private readonly CoreAudioController m_CoreAudioController = new CoreAudioController();

        private MainForm m_MainForm;

        private SoundPlayer m_PlayerA;
        private SoundPlayer m_PlayerB;

        private SoundPlayer m_PlayerRickRoll;
        private SoundPlayer m_PlayerInception;

        private bool m_ShowStopImage;
        private bool m_RickRolled;

        private Random m_Random;
        private int m_RandomSeed;

        private DateTime m_StartTime;
        private TimeSpan m_RandomDelay;

        private int m_Type;

        public int type { get => m_Type; set => m_Type = value; }

        public int randomSeed
        {
            get => m_RandomSeed;
            set { m_RandomSeed = value; m_Random = new Random(m_RandomSeed); }
        }

        public MusicThread(MainForm mainForm)
        {
            m_MainForm = mainForm;

            var completePreferencesPath = Path.Combine(APP_PATH, PREFERENCES_PATH);
            if (File.Exists(completePreferencesPath))
            {
                using (var reader = new StreamReader(completePreferencesPath))
                {
                    m_Type = int.Parse(reader.ReadLine());
                    m_RandomSeed = int.Parse(reader.ReadLine());
                    m_Random = new Random(m_RandomSeed);
                }
            }
            else
            {
                m_RandomSeed = 0;
                m_Random = new Random(m_RandomSeed);

                m_Type = m_Random.Next(0, 2);
            }

            var musicPath = Path.Combine(APP_PATH, "Content\\Music\\");

            var soundPathA = musicPath + "WhatsNewPussycat.wav";
            m_PlayerA = new SoundPlayer { SoundLocation = soundPathA };

            m_PlayerA.Load();

            var soundPathB = musicPath + "WhoaWhoaWhoaWhoa.wav";
            m_PlayerB = new SoundPlayer { SoundLocation = soundPathB };

            m_PlayerB.Load();

            var soundPathRickRoll = musicPath + "NeverGunnaGiveYouUp.wav";
            m_PlayerRickRoll = new SoundPlayer { SoundLocation = soundPathRickRoll };

            m_PlayerRickRoll.Load();

            var soundPathInception = musicPath + "Inception.wav";
            m_PlayerInception = new SoundPlayer { SoundLocation = soundPathInception };

            m_PlayerInception.Load();
        }

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool altTab);

        protected override void UpdateLoop()
        {
            if (m_RandomDelay != TimeSpan.Zero)
                m_MainForm.remainingTimeLabelField.Invoke(
                    (MethodInvoker)delegate
                    {
                        m_MainForm.remainingTimeLabelField
                        .Text =
                            m_StartTime.Add(m_RandomDelay).Subtract(DateTime.Now).ToString();
                    });

            // Confirm it's not the weekend
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday ||
                DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                return;


            // Confirm it's during camp hours
            if (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 17)
            {
                m_ShowStopImage = false;

                if (m_RandomDelay == TimeSpan.Zero)
                    ResetTimer();

                var lunchTimeStart =
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                var lunchTimeEnd = lunchTimeStart.Add(new TimeSpan(0, 15, 0));

                var activityTimeStart =
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 30, 0);
                var activityTimeEnd = activityTimeStart.Add(new TimeSpan(0, 30, 0));

                // Rick Roll hours
                if (DateTime.Now >= lunchTimeStart && DateTime.Now <= lunchTimeEnd ||
                    DateTime.Now >= activityTimeStart && DateTime.Now <= activityTimeEnd)
                {
                    var processCount = Process.GetProcessesByName("chrome").Length;
                    if (!m_RickRolled || processCount == 0)
                    {
                        Process.Start(
                            "https://4cdn.hu/kraken/image/upload/s--TglD2Fmf--/w_1160/6xAfr45m0gweCG8Ja.gif");

                        // Wait one second
                        Thread.Sleep(1000);

                        var chrome = Process.GetProcessesByName("chrome");
                        SwitchToThisWindow(chrome.First().Handle, false);

                        SendKeys.SendWait("{F11}");

                        m_CoreAudioController.DefaultPlaybackDevice.Mute(false);
                        m_CoreAudioController.DefaultPlaybackDevice.Volume = MAX_VOLUME;

                        m_PlayerRickRoll.PlayLooping();

                        m_RickRolled = true;
                    }
                }
                else if (DateTime.Now >= m_StartTime.Add(m_RandomDelay))
                {
                    m_CoreAudioController.DefaultPlaybackDevice.Mute(false);
                    m_CoreAudioController.DefaultPlaybackDevice.Volume = MAX_VOLUME;

                    if (m_Type == 0)
                        m_PlayerA.PlaySync();
                    if (m_Type == 1)
                        m_PlayerB.PlaySync();

                    ResetTimer();
                }
                else if (m_RickRolled)
                {
                    m_PlayerRickRoll.Stop();

                    m_RickRolled = false;
                }
            }
            else if (DateTime.Now.Hour < 8 || DateTime.Now.Hour > 17)
            {
                var processCount = Process.GetProcessesByName("chrome").Length;
                if (!m_ShowStopImage || processCount == 0)
                {
                    Process.Start("http://i.onionstatic.com/avclub/5150/69/16x9/1200.jpg");

                    Thread.Sleep(1000);

                    var chrome = Process.GetProcessesByName("chrome");
                    SwitchToThisWindow(chrome.First().Handle, false);

                    SendKeys.SendWait("{F11}");

                    m_CoreAudioController.DefaultPlaybackDevice.Mute(false);
                    m_CoreAudioController.DefaultPlaybackDevice.Volume = MAX_VOLUME;

                    m_PlayerInception.PlaySync();

                    m_ShowStopImage = true;
                }
            }
        }

        protected override void OnExitLoop()
        {
            m_PlayerRickRoll.Stop();

            using (var writer = new StreamWriter(PREFERENCES_PATH, false))
            {
                writer.WriteLine(m_Type);
                writer.WriteLine(m_RandomSeed);
            }
        }

        private void ResetTimer()
        {
            var randomSeconds = m_Random.Next(0, 60);
            var randomMinutes = m_Random.Next(0, 45);

            m_RandomDelay = new TimeSpan(0, 1, randomMinutes, randomSeconds);

            m_StartTime = DateTime.Now;
        }
    }
}
