using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace MouseHalo.Effects
{
    public class BaseEffectWindow : Window
    {
        public BaseEffectWindow() : base()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowChrome.SetWindowChrome(this, new WindowChrome
            {
                GlassFrameThickness = new Thickness(-1),
            });
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            // avoid get focus when loaded
            Focusable = false;
            ShowActivated = false;
        }

        public Point Offset { get; set; } = new Point(0, 0);
        
        public void FollowCursor()
        {
            Left = System.Windows.Forms.Cursor.Position.X - ActualWidth / 2 + Offset.X;
            Top = System.Windows.Forms.Cursor.Position.Y - ActualHeight / 2 + Offset.Y;
        }

        public void CloseAfter(TimeSpan timeSpan)
        {
            Task.Delay(timeSpan).ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Close();
                });
            });
        }
    }
}
