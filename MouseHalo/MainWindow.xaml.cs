using MouseHalo.Effects;
using MouseHalo.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MouseHalo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ConfigWindow? configWindow;
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void OpenConfigWindow(object sender, EventArgs e)
        {
            if (configWindow == null)
            {
                var window = new ConfigWindow();
                window.Show();
                configWindow = window;
                configWindow.Closing += (o, e) =>
                {
                    configWindow = null;
                };
            }
            else
            {
                configWindow.Show();
            }
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainTaskbarIcon.Visibility = Visibility.Hidden;
        }

        private void IsEnableMenuItemChecked(object sender, RoutedEventArgs e)
        {
            App.Enable = true;
        }

        private void IsEnableMenuItemUnchecked(object sender, RoutedEventArgs e)
        {
            App.Enable = false;
        }
    }
}
