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
    using FMOD;

    public class MusicThread : DefaultThread
    {
        private const int MAX_VOLUME = 30;

        private const string PREFERENCES_PATH = "preferences.txt";

        private CoreAudioController m_CoreAudioController;

        private MainForm m_MainForm;

        private Channel m_Channel;

        private Sound m_SoundA;
        private Sound m_SoundB;

        private Sound m_SoundSomebody;

        private Sound m_SoundNeverSawItComing;
        private Sound m_SoundSurpriseAttack;
        private Sound m_SoundPersonaActivate;
        private Sound m_SoundAllEnemiesDefeated;

        private Sound[] m_SoundsMorgana;

        private Sound m_SoundInception;

        private bool m_ShowStopImage;
        private bool m_NeverSawItComing;
        private bool m_Defeated;

        private Random m_Random;
        private int m_RandomSeed;

        private DateTime m_StartTime;
        private TimeSpan m_RandomDelay;

        private DateTime m_LastSupriseStartTime;
        private TimeSpan m_LastSupriseDelay;

        private int m_Type;


        public int type { get => m_Type; set => m_Type = value; }

        public int randomSeed
        {
            get => m_RandomSeed;
            set { m_RandomSeed = value; SetRandom(); }
        }

        public MusicThread(MainForm mainForm, int loopDelay = 66)
        {
            m_MainForm = mainForm;

            m_LoopDelay = loopDelay;
        }

        protected override void OnStartLoop()
        {
            var completePreferencesPath = Path.Combine(APP_PATH, PREFERENCES_PATH);
            if (File.Exists(completePreferencesPath))
            {
                using (var reader = new StreamReader(completePreferencesPath))
                {
                    m_Type = int.Parse(reader.ReadLine());
                    m_RandomSeed = int.Parse(reader.ReadLine());
                    SetRandom();
                }
            }
            else
            {
                m_RandomSeed = 0;
                SetRandom();

                m_Type = m_Random.Next(0, 2);
            }

            m_CoreAudioController = new CoreAudioController();

            Audio.Init();

            var musicPath = Path.Combine(APP_PATH, "Content\\Music\\");

            var soundPathA = musicPath + "WhatsNewPussycat.wav";
            Audio.LoadSound(soundPathA, out m_SoundA);

            var soundPathB = musicPath + "WhoaWhoaWhoaWhoa.wav";
            Audio.LoadSound(soundPathB, out m_SoundB);

            var soundPathSomebody = musicPath + "Somebody.wav";
            Audio.LoadSound(soundPathSomebody, out m_SoundSomebody);

            var soundPathNeverSawItComing = musicPath + "NeverSeeItComing.wav";
            Audio.LoadSound(soundPathNeverSawItComing, out m_SoundNeverSawItComing);

            var soundPathSurpriseAttack = musicPath + "SurpriseAttack.wav";
            Audio.LoadSound(soundPathSurpriseAttack, out m_SoundSurpriseAttack);

            var soundPathPersonaActivate = musicPath + "PersonaActivate.wav";
            Audio.LoadSound(soundPathPersonaActivate, out m_SoundPersonaActivate);

            var soundPathAllEnemiesDefeated = musicPath + "AllEnemiesDefeated.wav";
            Audio.LoadSound(soundPathAllEnemiesDefeated, out m_SoundAllEnemiesDefeated);

            var morganaSounds = Directory.GetFiles(musicPath + "Morgana\\");

            m_SoundsMorgana = new Sound[morganaSounds.Length];
            for (var i = 0; i < morganaSounds.Length; i++)
                Audio.LoadSound(morganaSounds[i], out m_SoundsMorgana[i]);

            var soundPathInception = musicPath + "Inception.wav";
            Audio.LoadSound(soundPathInception, out m_SoundInception);

            Audio.LoadChannel(m_SoundInception, out m_Channel);

            // Wait for form to load...?
            Thread.Sleep(1000);
        }

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool altTab);

        protected override void UpdateLoop()
        {
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
                    if (!m_NeverSawItComing || processCount == 0)
                    {
                        SetVolume();

                        Audio.PlaySound(ref m_Channel, m_SoundSurpriseAttack, MODE.DEFAULT, 0);

                        Process.Start(
                            "https://giant.gfycat.com/EvenWhoppingAlabamamapturtle.gif");

                        // Wait one second
                        Thread.Sleep(1000);

                        var chrome = Process.GetProcessesByName("chrome");
                        SwitchToThisWindow(chrome.First().Handle, false);

                        SendKeys.SendWait("{F11}");

                        Thread.Sleep(1000);

                        Audio.LoadChannel(m_SoundPersonaActivate, out Channel newChannel);
                        Audio.PlaySound(ref newChannel, m_SoundPersonaActivate, MODE.DEFAULT, 0);

                        Thread.Sleep(500);

                        m_Channel.setPitch(1f);
                        Audio.PlaySound(ref m_Channel, m_SoundNeverSawItComing, MODE.LOOP_NORMAL);

                        ResetLastSurpriseTimer();

                        m_NeverSawItComing = true;
                        m_Defeated = false;
                    }
                    else if (!m_Defeated && (lunchTimeEnd.Subtract(DateTime.Now) <= new TimeSpan(0, 0, 5) && lunchTimeEnd.Subtract(DateTime.Now) > TimeSpan.Zero ||
                             activityTimeEnd.Subtract(DateTime.Now) <= new TimeSpan(0, 0, 5) && activityTimeEnd.Subtract(DateTime.Now) > TimeSpan.Zero))
                    {
                        Audio.LoadChannel(m_SoundAllEnemiesDefeated, out Channel newChannel);

                        SetVolume();
                        Audio.PlaySound(ref newChannel, m_SoundAllEnemiesDefeated, MODE.DEFAULT, 0);

                        m_Defeated = true;
                    }
                    else if (DateTime.Now >= m_LastSupriseStartTime.Add(m_LastSupriseDelay))
                    {
                        var morganaSound = m_SoundsMorgana[m_Random.Next(0, m_SoundsMorgana.Length)];

                        Audio.LoadChannel(morganaSound, out Channel newChannel);
                        newChannel.setVolume(1.5f);

                        Audio.PlaySound(ref newChannel, morganaSound, MODE.DEFAULT, 0);

                        ResetLastSurpriseTimer();
                    }

                    m_MainForm.remainingTimeLabelField.Invoke(
                        (MethodInvoker)delegate
                        {
                            m_MainForm.remainingTimeLabelField
                                    .Text =
                                m_LastSupriseStartTime.Add(m_LastSupriseDelay).Subtract(DateTime.Now).ToString();
                        });
                }
                else if (DateTime.Now >= m_StartTime.Add(m_RandomDelay))
                {
                    SetVolume();

                    //if (m_Type == 0)
                    //    Audio.PlaySound(ref m_Channel, m_SoundA, MODE.DEFAULT, 0);
                    //if (m_Type == 1)
                    //    Audio.PlaySound(ref m_Channel, m_SoundB, MODE.DEFAULT, 0);

                    Audio.PlaySound(ref m_Channel, m_SoundSomebody, MODE.DEFAULT, 0);
                    m_Channel.setPitch(NextFloat(0.5f, 1.25f));

                    ResetTimer();
                }
                else if (m_NeverSawItComing)
                {
                    Audio.Stop(m_Channel);

                    m_NeverSawItComing = false;
                }
                else
                {
                    m_MainForm.remainingTimeLabelField.Invoke(
                        (MethodInvoker)delegate
                        {
                            m_MainForm.remainingTimeLabelField
                                    .Text =
                                m_StartTime.Add(m_RandomDelay).Subtract(DateTime.Now).ToString();
                        });
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

                    SetVolume();

                    m_Channel.setPitch(1f);
                    Audio.PlaySound(ref m_Channel, m_SoundInception, MODE.DEFAULT, 0);

                    m_ShowStopImage = true;
                }
            }

            Thread.Sleep(m_LoopDelay);
        }

        protected override void OnExitLoop()
        {
            Audio.Stop(m_Channel);

            using (var writer = new StreamWriter(PREFERENCES_PATH, false))
            {
                writer.WriteLine(m_Type);
                writer.WriteLine(m_RandomSeed);
            }
        }

        private void SetVolume()
        {
            m_CoreAudioController.DefaultPlaybackDevice.Mute(false);

            if (m_CoreAudioController.DefaultPlaybackDevice.Volume < MAX_VOLUME)
                m_CoreAudioController.DefaultPlaybackDevice.Volume = MAX_VOLUME;
        }

        private void ResetTimer()
        {
#if !DEBUG
            var randomSeconds = m_Random.Next(0, 60);
            var randomMinutes = m_Random.Next(0, 60);

            m_RandomDelay = new TimeSpan(0, 0, 60 + randomMinutes, randomSeconds);
#else
            var randomSeconds = m_Random.Next(0, 5);

            m_RandomDelay = new TimeSpan(0, 0, 0, 3 + randomSeconds);
#endif

            m_StartTime = DateTime.Now;
        }

        private void ResetLastSurpriseTimer()
        {
#if !DEBUG
            var randomSeconds = m_Random.Next(0, 15);

            m_LastSupriseDelay = new TimeSpan(0, 0, 0, 5 + randomSeconds);
#else
            var randomSeconds = m_Random.Next(0, 5);

            m_LastSupriseDelay = new TimeSpan(0, 0, 0, 3 + randomSeconds);
#endif

            m_LastSupriseStartTime = DateTime.Now;
        }

        private void SetRandom()
        {
            m_Random =
                m_RandomSeed != 0 ? new Random(m_RandomSeed) : new Random(Guid.NewGuid().GetHashCode());
        }

        float NextFloat(float min, float max)
        {
            return (float)NextDouble(min, max);
        }
        double NextDouble(double min, double max)
        {
            return min + m_Random.NextDouble() * (max - min);
        }
    }
}
