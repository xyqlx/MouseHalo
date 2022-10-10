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
using System.Configuration;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32.TaskScheduler;

namespace MouseHalo
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            this.DataContext = App.AppConfig;
            using (TaskService ts = new TaskService())
            {
                if(ts.RootFolder.SubFolders.FirstOrDefault(f => f.Name == "MouseHalo") != null)
                {
                    hasRegistered = true;
                    RegisterButon.Content = App.Current.Resources["unregister_task"];
                }
            }
        }

        private bool hasRegistered = false;

        // on load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.AppConfig.PropertyChanged += SaveConfig;
        }

        private void SaveConfig(object? sender, PropertyChangedEventArgs e)
        {
            Debug.Print(App.AppConfig.IsCheckShiftKeyPress.ToString());
            AppConfig.Save(App.AppConfig);
        }

        // on close
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            App.AppConfig.PropertyChanged -= SaveConfig;
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = Environment.ProcessPath;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            if (!hasRegistered)
            {
                proc.StartInfo.Arguments = "register";
            }
            else
            {
                proc.StartInfo.Arguments = "unregister";
            }
            RegisterButon.IsEnabled = false;
            proc.Start();
        }
    }
}
