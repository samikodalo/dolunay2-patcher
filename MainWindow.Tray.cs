using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace Dolunay2Launcher
{
    public partial class MainWindow
    {
        WinForms.NotifyIcon _tray;
        bool _reallyClose;
        bool _notified;
        readonly HashSet<string> _reminded = new HashSet<string>();
        DispatcherTimer _reminder;

        void InitTray()
        {
            try
            {
                _tray = new WinForms.NotifyIcon();
                _tray.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
                _tray.Text = "Dolunay2 Launcher";
                _tray.Visible = true;
                var menu = new WinForms.ContextMenuStrip();
                menu.Items.Add(T("tray.open"), null, (s, e) => ShowFromTray());
                menu.Items.Add(T("tray.exit"), null, (s, e) => { _reallyClose = true; Close(); });
                _tray.ContextMenuStrip = menu;
                _tray.DoubleClick += (s, e) => ShowFromTray();

                _reminder = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
                _reminder.Tick += (s, e) => CheckReminders();
                _reminder.Start();
            }
            catch (Exception) { }
        }

        void CheckReminders()
        {
            if (_mf == null || _tray == null || _settings == null || !_settings.Notifications) return;
            var now = DateTime.Now;
            foreach (var e in _mf.events.special)
            {
                DateTime start;
                if (!DateTime.TryParseExact(e.start_date + " " + e.start, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start)) continue;
                var diff = start - now;
                if (diff.TotalMinutes >= 0 && diff.TotalMinutes <= _settings.ReminderLead)
                {
                    string key = "s:" + e.start_date + e.start + e.title_tr;
                    if (_reminded.Add(key))
                        _tray.ShowBalloonTip(4000, T("tray.reminder"), (_en ? e.title_en : e.title_tr) + " · " + e.start, WinForms.ToolTipIcon.Info);
                }
            }
            int day = TodayDay();
            foreach (var e in _mf.events.routine)
            {
                if (e.day != day) continue;
                DateTime start;
                if (!DateTime.TryParseExact(now.ToString("yyyy-MM-dd") + " " + e.start, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start)) continue;
                var diff = start - now;
                if (diff.TotalMinutes >= 0 && diff.TotalMinutes <= _settings.ReminderLead)
                {
                    string key = "r:" + now.ToString("yyyyMMdd") + e.start + e.title_tr;
                    if (_reminded.Add(key))
                        _tray.ShowBalloonTip(4000, T("tray.reminder"), (_en ? e.title_en : e.title_tr) + " · " + e.start, WinForms.ToolTipIcon.Info);
                }
            }
        }

        internal void ShowFromTray()
        {
            try
            {
                if (!IsVisible) Show();
                if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
                Activate();
                // Bazi durumlarda Windows on plana alma istegini engeller.
                Topmost = true;
                Topmost = false;
                Focus();
            }
            catch (Exception) { }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_reallyClose && _settings != null && _settings.CloseToTray)
            {
                e.Cancel = true;
                Hide();
                if (_tray != null && _settings.Notifications)
                    _tray.ShowBalloonTip(1500, "Dolunay2", T("tray.running"), WinForms.ToolTipIcon.Info);
                return;
            }
            if (_tray != null) { _tray.Visible = false; _tray.Dispose(); }
            base.OnClosing(e);
        }
    }
}
