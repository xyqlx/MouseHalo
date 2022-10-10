using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using NHotkey.Wpf;

namespace MouseHalo.Util
{
    public sealed class IMEManager
    {
        // TODO maybe add check to enter textbox?
        
        /// <summary>
        /// Occurs when the contents of the clipboard is updated.
        /// </summary>
        public static event EventHandler<IMEEventArgs?>? IMEUpdate;

        /// <summary>
        /// Raises the <see cref="IMEUpdate"/> event.
        /// </summary>
        /// <param name="e">Event arguments for the event.</param>
        private static void OnIMEUpdate(IMEEventArgs? e)
        {
            var handler = IMEUpdate;
            if (handler != null)
            {
                handler(null, e);
            }
        }

        private static TimeSpan? interval;
        public static TimeSpan? Interval
        {
            get => interval; set
            {
                interval = value;
                if(DetectTimer != null)
                {
                    DetectTimer.Stop();
                    DetectTimer = null;
                }
                if (value.HasValue && value.Value.TotalMilliseconds > 0)
                {
                    DetectTimer = new DispatcherTimer { Interval = value.Value };
                    DetectTimer.Tick += (o, e) => CheckIMEStatus(IMEEventSource.StatusChange);
                    DetectTimer.Start();
                }
            }
        }
        public static bool IsCheckWindowChange { get; set; } = false;
        public static bool IsCheckShiftKeyPress { get; set; } = false;
        public static DispatcherTimer? DetectTimer { get; set; }

        /// <summary>
        /// send message to foreground window's IME window for getting IME status
        /// </summary>
        private static void CheckIMEStatus(IMEEventSource eventSource)
        {
            // TODO 排查非管理员模式下有时候并不能及时检测的原因
            IMEStatusData data = new();
            var handle = NativeMethods.GetForegroundWindow();
            // Debug.Print($"foreground window: {handle}");
            var imeHandle = NativeMethods.ImmGetDefaultIMEWnd(handle);
            // Debug.Print($"ime window: {imeHandle}");
            var result = NativeMethods.SendMessage(imeHandle, NativeMethods.WM_IME_CONTROL, NativeMethods.IMC_GETCONVERSIONMODE, 0);
            // Debug.Print($"ime conversion mode: {result}");
            if ((result & NativeMethods.IME_CMODE_CHINESE) != 0)
            {
                data.IsChinese = true;
            }
            else
            {
                data.IsChinese = false;
            }
            OnIMEUpdate(new IMEEventArgs { IMEStatus = data, IMEHandle = imeHandle, WindowHandle = handle, IMEEventSource = eventSource });
        }

        public static TimeSpan ShiftDetectDelay = TimeSpan.FromMilliseconds(50);

        static IMEManager()
        {
            // foreground window change
            var hEvent = NativeMethods.SetWinEventHook(NativeMethods.EVENT_SYSTEM_FOREGROUND, NativeMethods.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, ForegroundChangeEventCallback, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT | NativeMethods.WINEVENT_SKIPOWNTHREAD);
            //HotkeyManager.Current.AddOrReplace("IMEManagerLeftShift", new KeyGesture(Key.Space, ModifierKeys.Control), (s, e) =>
            //{
            //    if (IsCheckShiftKeyPress)
            //    {
            //        CheckIMEStatus();
            //    }
            //});
            HotkeyManager.Current.AddOrReplace("Shift", Key.None, ModifierKeys.Shift, (o, e) =>
            {
                
                // wait for IME change status
                Task.Delay(ShiftDetectDelay).ContinueWith((t) =>
                {
                    if (IsCheckShiftKeyPress)
                    {
                        CheckIMEStatus(IMEEventSource.ShiftKey);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            });
        }

        private static NativeMethods.WinEventDelegate ForegroundChangeEventCallback = (IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) =>
        {
            if (IsCheckWindowChange)
            {
                CheckIMEStatus(IMEEventSource.WindowChange);
            }
        };
    }

    public enum IMEEventSource
    {
        WindowChange,
        ShiftKey,
        StatusChange
    }

    public class IMEEventArgs : EventArgs
    {
        public IMEStatusData IMEStatus { get; set; } = new IMEStatusData();
        public IntPtr? IMEHandle { get; set; }
        public IntPtr? WindowHandle { get; set; }
        public IMEEventSource IMEEventSource { get; set; }
    }

    public class IMEStatusData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private bool isChinese = false;
        public bool IsChinese
        {
            get => isChinese;
            set
            {
                isChinese = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChinese)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChineseText)));
            }
        }
        
        public string IsChineseText => IsChinese ? "🀄" : "🔤";
    }

    internal static partial class NativeMethods
    {
        public const uint WM_IME_CONTROL = 0x283;
        public const uint IMC_GETCONVERSIONMODE = 0x1;
        // https://www.rpi.edu/dept/cis/software/g77-mingw32/include/imm.h
        public const uint IME_CMODE_CHINESE = 0x3;
        public const uint IME_CMODE_SYMBOL = 0x400;

        [DllImport("imm32.dll", SetLastError = true)]
        public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint SendMessage(IntPtr hWnd, uint wMsg, uint wParam, uint lParam);

        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public const uint EVENT_SYSTEM_FOREGROUND = 0x3;
        public const uint WINEVENT_OUTOFCONTEXT = 0x0;
        public const uint WINEVENT_SKIPOWNTHREAD = 0x2;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
    }
}
