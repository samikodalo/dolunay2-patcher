using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Dolunay2Launcher
{
    public partial class App : Application
    {
        Mutex _singleMutex;
        EventWaitHandle _showEvent;
        MainWindow _mainWindow;
        string _mutexName;
        string _eventName;

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                try
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.txt"),
                        DateTime.Now + "\r\n" + ex.Exception);
                }
                catch (Exception) { }
                MessageBox.Show(ex.Exception.Message, "Dolunay2 Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
                Shutdown(1);
            };

            // Ayni anda yalnizca bir launcher calisabilir.
            // Ikinci kez acilirsa mevcut pencereyi one getirip bu sureci kapat.
            _mutexName = @"Local\Dolunay2Launcher_SingleInstance";
            _eventName = @"Local\Dolunay2Launcher_SingleInstance_Show";

            bool created = true;
            try
            {
                _singleMutex = new Mutex(true, _mutexName, out created);
            }
            catch (Exception)
            {
                // Mutex olusturulamazsa uygulamayi engelleme.
                created = true;
            }

            if (!created)
            {
                SignalExistingInstance();
                Shutdown();
                return;
            }

            try
            {
                _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, _eventName);
            }
            catch (Exception) { _showEvent = null; }

            base.OnStartup(e);
            try { ThemeManager.Apply("Default"); } catch (Exception) { }
            _mainWindow = new MainWindow();
            _mainWindow.Show();
            StartShowListener();
        }

        void SignalExistingInstance()
        {
            // Ilk surec event'i henuz olusturmadiysa kisa sure dene.
            for (int i = 0; i < 12; i++)
            {
                try
                {
                    using (var ev = EventWaitHandle.OpenExisting(_eventName))
                    {
                        ev.Set();
                        return;
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }

        void StartShowListener()
        {
            if (_showEvent == null) return;
            var thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        _showEvent.WaitOne();
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                if (_mainWindow != null) _mainWindow.ShowFromTray();
                            }
                            catch (Exception) { }
                        }));
                    }
                    catch (Exception) { break; }
                }
            })
            {
                IsBackground = true,
                Name = "Dolunay2SingleInstance"
            };
            thread.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_showEvent != null)
                {
                    _showEvent.Set();
                    _showEvent.Dispose();
                }
            }
            catch (Exception) { }
            try
            {
                if (_singleMutex != null)
                {
                    _singleMutex.ReleaseMutex();
                    _singleMutex.Dispose();
                }
            }
            catch (Exception) { }
            base.OnExit(e);
        }
    }
}
