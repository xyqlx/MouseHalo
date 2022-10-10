using Microsoft.Win32.TaskScheduler;
using MouseHalo.Effects;
using MouseHalo.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MouseHalo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AppConfig AppConfig { get; set; } = AppConfig.Load();
        public static bool Enable { get; set; } = true;

        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            SetLanguageDictionary();
            // SetNotifyIcon();

        }

        private IMEEffectWindow? IMEEffectWindow;
        private IntPtr? IMEEffectWindowHandle;
        private readonly Mutex Mutex = new Mutex(true, "MouseHalo");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                if (e.Args.FirstOrDefault() == "register")
                {
                    RegisterTask();
                }
                else if(e.Args.FirstOrDefault() == "unregister")
                {
                    UnRegisterTask();
                }
                Environment.Exit(0);
            }
            // check if is already running
            if (!Mutex.WaitOne(TimeSpan.Zero, true))
            {
                //already an instance running
                // create a dialog
                MessageBox.Show("another instance is already running");
                Environment.Exit(0);
            }
            var mainWindow = new MainWindow();
            mainWindow.Show();
            ClipboardManager.ClipboardUpdate += (o, e) =>
            {
                if (!Enable)
                {
                    return;
                }
                var window = new CopyEffectWindow(e);
                // show window at mouse position            
                window.Show();
            };
            IMEManager.Interval = TimeSpan.FromMilliseconds(AppConfig.ActiveCheckInterval);
            IMEManager.IsCheckWindowChange = AppConfig.IsCheckWindowChange;
            IMEManager.IsCheckShiftKeyPress = AppConfig.IsCheckShiftKeyPress;
            IntPtr? lastImeWindowHandle = null;
            bool? lastIsChinese = null;

            IMEManager.IMEUpdate += (o, e) =>
            {
                if (!Enable)
                {
                    return;
                }
                if (e == null)
                {
                    return;
                }
                // avoid check notification window
                if (e.WindowHandle == IMEEffectWindowHandle)
                {
                    return;
                }
                if (e.IMEEventSource == IMEEventSource.WindowChange || e.IMEEventSource == IMEEventSource.ShiftKey || lastImeWindowHandle != e.IMEHandle || lastIsChinese != e.IMEStatus.IsChinese)
                {
                    // Debug.Print($"Win: {e.WindowHandle:X}");
                    // Debug.Print($"IME: {e.IMEHandle:X}");
                    // Debug.Print(e.IMEStatus.IsChineseText);
                    // Debug.Print($"create window: {new WindowInteropHelper(window).Handle:X}");
                    if (IMEEffectWindow == null)
                    {
                        IMEEffectWindow = new IMEEffectWindow(e.IMEStatus);
                        IMEEffectWindow.Owner = mainWindow;
                        // Debug.Print($"handle {IMEEffectWindowHandle:X}");
                    }
                    else
                    {
                        IMEEffectWindow.IMEStatus.IsChinese = e.IMEStatus.IsChinese;
                    }
                    IMEEffectWindow.FollowCursor();
                    IMEEffectWindow.HideAfter(TimeSpan.FromSeconds(1));
                    IMEEffectWindow.Show();
                    if (!IMEEffectWindowHandle.HasValue)
                    {
                        IMEEffectWindowHandle = new WindowInteropHelper(IMEEffectWindow).Handle;
                    }
                }
                lastImeWindowHandle = e.IMEHandle;
                lastIsChinese = e.IMEStatus.IsChinese;
            };
        }

        public void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
            {
                case "zh":
                    dict.Source = new Uri("Resources/Language/zh-CHS.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Resources/Language/zh-CHS.xaml", UriKind.Relative);
                    break;
            }
            Resources.MergedDictionaries.Add(dict);
        }

        private void RegisterTask() {
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "start MouseHalo on login";

                // Create a trigger that will fire the task at this time every other day
                td.Triggers.Add(new LogonTrigger() { UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name });

                // Create an action that will launch Cocoa whenever the trigger fires
                var programPath = Environment.ProcessPath;
                td.Actions.Add(new ExecAction(programPath, workingDirectory: System.IO.Path.GetDirectoryName(programPath)));
                td.Principal.RunLevel = TaskRunLevel.Highest;

                // Register the task in the root folder
                var folder = ts.RootFolder.SubFolders.FirstOrDefault(f => f.Name == "MouseHalo") ?? ts.RootFolder.CreateFolder("MouseHalo");
                folder.DeleteTask("MouseHalo Auto Run", exceptionOnNotExists: false);
                folder.RegisterTaskDefinition("MouseHalo Auto Run", td);
            }
        }
        private void UnRegisterTask()
        {
            using TaskService ts = new TaskService();
            var folder = ts.RootFolder.SubFolders.FirstOrDefault(f => f.Name == "MouseHalo");
            if(folder != null)
            {
                folder.DeleteTask("MouseHalo Auto Run", exceptionOnNotExists: false);
                ts.RootFolder.DeleteFolder("MouseHalo");
            }

        }
    }
}
