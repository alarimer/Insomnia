/*  Copyright (C) {2017}  AwFuL Productions (Alan F Larimer)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Configuration;
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
            if (int.TryParse(ConfigurationManager.AppSettings["interval"], out int interval))
            {
                // convert minutes to milliseconds
                interval *= 60000;
            }
            else
            {
                // default to three minutes
                interval = 180000;
            }

            _timer = new System.Timers.Timer(interval);
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
