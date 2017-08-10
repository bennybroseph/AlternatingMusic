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
    public partial class MainForm : Form
    {
        private Random m_Random;
        private int m_RandomSeed;

        private TimeSpan m_RandomDelay;
        private DateTime m_StartTime;

        private bool m_AllowVisible;     // ContextMenu's Show command used
        private bool m_AllowClose;       // ContextMenu's Exit command used

        private int m_Type;

        private string m_PreferencesPath = "preferences.txt";
        private string m_InstallPath = "installData.json";

        private const int MAX_VOLUME = 30;

        public bool allowClose { get { return m_AllowClose; } set { m_AllowClose = value; } }

        public MainForm()
        {
            InitializeComponent();
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            showToolStripMenuItem.Click += showToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;

            var path = Directory.GetCurrentDirectory() + "\\" + m_PreferencesPath;
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    m_RandomSeed = int.Parse(reader.ReadLine());
                    m_Random = new Random(m_RandomSeed);
                    m_Type = int.Parse(reader.ReadLine());
                }
            }
            else
            {
                m_RandomSeed = 0;
                m_Random = new Random(m_RandomSeed);
                m_Type = m_Random.Next(0, 2);
            }

            radioButtonA.Checked = m_Type == 0;
            radioButtonB.Checked = m_Type == 1;

            seedTextBox.Text = m_RandomSeed.ToString();

            var sleeperThread = new Thread(SleeperThread);
            sleeperThread.Start();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!m_AllowVisible)
            {
                value = false;
                if (!IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!m_AllowClose)
            {
                Hide();
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_AllowVisible = true;
            Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void radioButtonA_CheckedChanged(object sender, EventArgs e)
        {
            m_Type = radioButtonA.Checked ? 0 : 1;
        }

        private void radioButtonB_CheckedChanged(object sender, EventArgs e)
        {
            m_Type = radioButtonB.Checked ? 1 : 0;
        }

        private void seedTextBox_TextChanged(object sender, EventArgs e)
        {
            m_RandomSeed = int.Parse(seedTextBox.Text);

            m_Random = new Random(m_RandomSeed);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "#Gucci2k17")
                return;

            DefaultThread._emergencyAbort = true;
            EmergencyExitOtherProcess();

            Exit();
        }

        private void ResetTimer()
        {
            var randomSeconds = m_Random.Next(0, 60);
            var randomMinutes = m_Random.Next(0, 45);

            m_RandomDelay = new TimeSpan(0, 1, randomMinutes, randomSeconds);

            m_StartTime = DateTime.Now;
        }

        private void Exit()
        {
            m_AllowClose = true;

            var path = Directory.GetCurrentDirectory() + "\\" + m_PreferencesPath;

            using (var writer = new StreamWriter(path, false))
            {
                writer.WriteLine(m_Type);
                writer.WriteLine(m_RandomSeed);
            }

            Application.Exit();
        }

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool altTab);

        private void SleeperThread()
        {
            try
            {
                SleeperLoop();
            }
            catch (Exception exception)
            {
                ErrorHandling.HandleException(exception);
            }
        }

        private void SleeperLoop()
        {
            var coreAudioController = new CoreAudioController();

            var musicPath = "Content\\Music\\";
            var soundPathA = musicPath + "WhatsNewPussycat.wav";
            var playerA = new SoundPlayer { SoundLocation = soundPathA };

            playerA.Load();

            var soundPathB = Directory.GetCurrentDirectory() + "\\Content\\Music\\WhoaWhoaWhoaWhoa.wav";
            var playerB = new SoundPlayer { SoundLocation = soundPathB };

            playerB.Load();

            var soundPathRickRoll =
                Directory.GetCurrentDirectory() + "\\Content\\Music\\NeverGunnaGiveYouUp.wav";
            var playerRickRoll = new SoundPlayer { SoundLocation = soundPathRickRoll };

            playerRickRoll.Load();

            var soundPathInception =
                Directory.GetCurrentDirectory() + "\\Content\\Music\\Inception.wav";
            var playerInception = new SoundPlayer { SoundLocation = soundPathInception };

            playerInception.Load();

            var showStopImage = false;
            var rickRolled = false;
            while (!m_AllowClose)
            {
                remainingTimeLabel.Invoke(
                    (MethodInvoker)delegate
                    {
                        remainingTimeLabel.Text =
                            m_StartTime.Add(m_RandomDelay).Subtract(DateTime.Now).ToString();
                    });

                // Confirm it's not the weekend
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday ||
                    DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    continue;


                // Confirm it's during camp hours
                if (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 17)
                {
                    showStopImage = false;

                    if (m_RandomDelay == TimeSpan.Zero)
                        ResetTimer();

                    var lunchTimeStart =
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    var lunchTimeEnd = lunchTimeStart.Add(new TimeSpan(0, 15, 0));

                    var activityTimeStart =
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    var activityTimeEnd = lunchTimeStart.Add(new TimeSpan(0, 15, 0));

                    // Rick Roll hours
                    if (DateTime.Now >= lunchTimeStart && DateTime.Now <= lunchTimeEnd ||
                        DateTime.Now >= activityTimeStart && DateTime.Now <= activityTimeEnd)
                    {
                        var processCount = Process.GetProcessesByName("chrome").Length;
                        if (!rickRolled || processCount == 0)
                        {
                            Process.Start(
                                "https://www.google.com/url?sa=i&rct=j&q=&esrc=s&" +
                                "source=images&cd=&ved=0ahUKEwiTgpWd4MrVAhUIh1QKHegpB2cQjBwIBA&" +
                                "url=https%3A%2F%2F4cdn.hu%2Fkraken%2Fimage%2Fupload%2Fs--TglD2Fmf--%" +
                                "2Fw_1160%2F6xAfr45m0gweCG8Ja.gif&psig=AFQjCNHl6kmRJ-" +
                                "V_1KlWVUqk9GfnbKZ59A&ust=1502388708515261");

                            // Wait one second
                            Thread.Sleep(1000);

                            var chrome = Process.GetProcessesByName("chrome");
                            SwitchToThisWindow(chrome.First().Handle, false);

                            SendKeys.SendWait("{F11}");

                            coreAudioController.DefaultPlaybackDevice.Mute(false);
                            coreAudioController.DefaultPlaybackDevice.Volume = MAX_VOLUME;

                            playerRickRoll.PlayLooping();

                            rickRolled = true;
                        }
                    }
                    else if (DateTime.Now >= m_StartTime.Add(m_RandomDelay))
                    {
                        coreAudioController.DefaultPlaybackDevice.Mute(false);
                        coreAudioController.DefaultPlaybackDevice.Volume = MAX_VOLUME;

                        if (m_Type == 0)
                            playerA.PlaySync();
                        if (m_Type == 1)
                            playerB.PlaySync();

                        ResetTimer();
                    }
                    else if (rickRolled)
                    {
                        playerRickRoll.Stop();

                        rickRolled = false;
                    }
                }
                else if (DateTime.Now.Hour < 8 || DateTime.Now.Hour > 17)
                {
                    var processCount = Process.GetProcessesByName("chrome").Length;
                    if (!showStopImage || processCount == 0)
                    {
                        Process.Start("http://i.onionstatic.com/avclub/5150/69/16x9/1200.jpg");

                        Thread.Sleep(1000);

                        var chrome = Process.GetProcessesByName("chrome");
                        SwitchToThisWindow(chrome.First().Handle, false);

                        SendKeys.SendWait("{F11}");

                        coreAudioController.DefaultPlaybackDevice.Mute(false);
                        coreAudioController.DefaultPlaybackDevice.Volume = MAX_VOLUME;

                        playerInception.PlaySync();

                        showStopImage = true;
                    }
                }

                Thread.Sleep(66);
            }

            playerRickRoll.Stop();
        }

        private void EmergencyExitOtherProcess()
        {
            var processes = Process.GetProcessesByName("DontStopMeNow");
            foreach (var process in processes)
                process.Kill();
        }
    }
}
