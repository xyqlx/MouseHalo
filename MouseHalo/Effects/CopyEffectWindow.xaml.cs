using MouseHalo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class CopyEffectWindow : BaseEffectWindow
    {
        public CopyEffectWindow(ClipboardEventArgs? eventArgs) : base()
        {
            InitializeComponent();
            SetLabel(eventArgs);
            FollowCursor();
            CloseAfter(TimeSpan.FromSeconds(1));
        }

        public void SetLabel(ClipboardEventArgs? eventArgs)
        {
            if (eventArgs == null)
            {
                return;
            }
            var data = eventArgs.Data;
            if(data != null)
            {
                // some data formats see https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.dataformats?view=windowsdesktop-6.0
                // next line will raise exception in some cases
                // var formats = data.GetFormats();


                List<string> types = new List<string>();
                // File
                if (data.GetDataPresent(DataFormats.FileDrop))
                {
                    types.Add("📁");
                }
                // Image
                if (data.GetDataPresent(DataFormats.Bitmap))
                {
                    types.Add("🖼");
                }
                // HTML
                if (data.GetDataPresent(DataFormats.Html))
                {
                    types.Add("🕸");
                }
                // Text
                if (data.GetDataPresent(DataFormats.Rtf))
                {
                    types.Add("📗");
                }else if(data.GetDataPresent(DataFormats.UnicodeText) || data.GetDataPresent(DataFormats.Text))
                {
                    types.Add("📄");
                }
                // other
                if(types.Count == 0)
                {
                    types.Add("❓︎");
                }
                NoticeTextBlock.FontSize = App.AppConfig.FontSize;
                NoticeTextBlock.Text = string.Join("", types);
                Offset = new Point(10, -NoticeTextBlock.FontSize - 20);
            }
        }
    }
}
