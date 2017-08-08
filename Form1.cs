using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using AudioSwitcher.AudioApi.CoreAudio;

namespace AlternatingMusic
{
    public partial class Form1 : Form
    {
        private Random m_Random;
        private int m_RandomSeed;

        private TimeSpan m_RandomDelay;
        private DateTime m_StartTime;

        private bool m_AllowVisible;     // ContextMenu's Show command used
        private bool m_AllowClose;       // ContextMenu's Exit command used

        private int m_Type;

        private string m_FilePath = "preferences.txt";

        public Form1()
        {
            InitializeComponent();
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            showToolStripMenuItem.Click += showToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;

            var path = Directory.GetCurrentDirectory() + "\\" + m_FilePath;
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

            var dontStopMeNow = new Thread(DontStopMeNow);
            dontStopMeNow.Start();
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

            foreach (var process in Process.GetProcessesByName("DontStopMeNow"))
                process.Kill();

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

            var path = Directory.GetCurrentDirectory() + "\\" + m_FilePath;

            using (var writer = new StreamWriter(path, false))
            {
                writer.WriteLine(m_Type);
                writer.WriteLine(m_RandomSeed);
            }

            Application.Exit();
        }

        private void SleeperThread()
        {
            var coreAudioController = new CoreAudioController();

            var soundPathA = Directory.GetCurrentDirectory() + "\\Content\\Music\\WhatsNewPussycat.wav";
            var playerA = new SoundPlayer { SoundLocation = soundPathA };

            playerA.Load();

            var soundPathB = Directory.GetCurrentDirectory() + "\\Content\\Music\\WhoaWhoaWhoaWhoa.wav";
            var playerB = new SoundPlayer { SoundLocation = soundPathB };

            playerB.Load();

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
                if (true)
                //if (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 17)
                {
                    if (m_RandomDelay == TimeSpan.Zero)
                        ResetTimer();

                    if (DateTime.Now >= m_StartTime.Add(m_RandomDelay))
                    {
                        coreAudioController.DefaultPlaybackDevice.Mute(false);
                        coreAudioController.DefaultPlaybackDevice.Volume = 30;

                        if (m_Type == 0)
                            playerA.PlaySync();
                        if (m_Type == 1)
                            playerB.PlaySync();

                        ResetTimer();
                    }
                }

                Thread.Sleep(66);
            }
        }

        private void DontStopMeNow()
        {
            while (!m_AllowClose)
            {
                var processCount = Process.GetProcessesByName("DontStopMeNow").Length;
                if (processCount == 0)
                    Process.Start("..\\Updater\\DontStopMeNow.exe");

                Thread.Sleep(500);
            }
        }
    }
}
