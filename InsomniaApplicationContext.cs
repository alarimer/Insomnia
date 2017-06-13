using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Insomnia
{
    class InsomniaApplicationContext : ApplicationContext
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private const string NO_SLEEPING = "No sleeping";
        private const string ALLOW_SLEEP = "Allow sleep";

        private System.ComponentModel.IContainer _components;
        private static System.Timers.Timer _timer;
        private NotifyIcon _trayIcon;

        public InsomniaApplicationContext()
        {
            _components = new System.ComponentModel.Container();
            InitializeComponent();
            _trayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            // three minutes
            _timer = new System.Timers.Timer(180000);
            _timer.Elapsed += OnTimeElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _components.Add(_timer);

            ContextMenuStrip _contextMenu = new ContextMenuStrip();

            ToolStripMenuItem toggleItem = new ToolStripMenuItem()
            {
                Name = "Toggle",
                Text = ALLOW_SLEEP
            };
            toggleItem.Click += new EventHandler(ToggleItem_Click);
            _contextMenu.Items.Add(toggleItem);

            ToolStripMenuItem exitItem = new ToolStripMenuItem()
            {
                Name = "Exit",
                Text = "Exit"
            };
            exitItem.Click += new EventHandler(ExitItem_Click);
            _contextMenu.Items.Add(exitItem);

            _trayIcon = new NotifyIcon(_components)
            {
                Text = NO_SLEEPING,
                Icon = Properties.Resources.InsomniaIcon,
                ContextMenuStrip = _contextMenu
            };
        }

        private static void OnTimeElapsed(Object sender, System.Timers.ElapsedEventArgs e)
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void ToggleItem_Click(Object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (item.Text == ALLOW_SLEEP)
            {
                item.Text = NO_SLEEPING;
                _trayIcon.Text = ALLOW_SLEEP;
                _timer.Stop();
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
            else
            {
                item.Text = ALLOW_SLEEP;
                _trayIcon.Text = NO_SLEEPING;
                _timer.Start();
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
        }

        private void ExitItem_Click(Object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

            if (_timer != null)
            {
                _timer.Stop();
            }

            if (disposing)
            {
                if (_components != null)
                {
                    _components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002
        }
    }
}
