using System;
using System.Diagnostics;
using System.Windows.Forms;
using SuperLibrary;

namespace SuperNetNanny
{
    public partial class MainForm : Form
    {
        private bool m_AllowVisible;     // ContextMenu's Show command used
        private bool m_AllowClose;       // ContextMenu's Exit command used

        private MusicThread m_MusicThread;

        public Label remainingTimeLabelField { get => remainingTimeLabel; }

        public RadioButton radioButtonAField { get => radioButtonA; }
        public RadioButton radioButtonBField { get => radioButtonB; }

        public TextBox seedTextBoxField { get => seedTextBox; }

        public MainForm()
        {
            InitializeComponent();

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            showToolStripMenuItem.Click += showToolStripMenuItem_Click;

            m_MusicThread = new MusicThread(this);
            m_MusicThread.StartThread();

            radioButtonAField.Checked = m_MusicThread.type == 0;
            radioButtonBField.Checked = m_MusicThread.type == 1;

            seedTextBoxField.Text = m_MusicThread.randomSeed.ToString();
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

        private void radioButtonA_CheckedChanged(object sender, EventArgs e)
        {
            m_MusicThread.type = radioButtonA.Checked ? 0 : 1;
        }

        private void radioButtonB_CheckedChanged(object sender, EventArgs e)
        {
            m_MusicThread.type = radioButtonB.Checked ? 1 : 0;
        }

        private void seedTextBox_TextChanged(object sender, EventArgs e)
        {
            m_MusicThread.randomSeed = int.Parse(seedTextBox.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "#Gucci2k17")
                return;

            m_AllowClose = true;

            DefaultThread._emergencyAbort = true;
            EmergencyExitOtherProcess();

            Application.Exit();
        }

        private void EmergencyExitOtherProcess()
        {
            var processes = Process.GetProcessesByName("DontStopMeNow");
            foreach (var process in processes)
                process.Kill();
        }
    }
}
