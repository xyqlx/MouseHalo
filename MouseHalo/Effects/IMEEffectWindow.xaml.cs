﻿using MouseHalo.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MouseHalo.Effects
{
    /// <summary>
    /// CopyEffectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IMEEffectWindow : BaseEffectWindow
    {
        public IMEEffectWindow(IMEStatusData IMEStatus): base()
        {
            this.IMEStatus = IMEStatus;
            this.DataContext = this.IMEStatus;
            InitializeComponent();
            NoticeLabel.FontSize = App.AppConfig.FontSize;
            Offset = new Point(30, 30);
        }

        public IMEStatusData IMEStatus = new();
        private CancellationTokenSource? cancellationTokenSource;

        public void HideAfter(TimeSpan timeSpan)
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            Task.Delay(timeSpan, cancellationToken).ContinueWith((t) =>
            {
                Debug.Print(t.IsCompletedSuccessfully.ToString());
                if (t.IsCompletedSuccessfully)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Hide();
                    });
                }
            });
        }
    }
}
