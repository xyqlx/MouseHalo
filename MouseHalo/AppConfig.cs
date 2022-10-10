using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MouseHalo
{
    public class AppConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private bool isCheckWindowChange = true;
        public bool IsCheckWindowChange { get => isCheckWindowChange; set {
                isCheckWindowChange = value;
                OnPropertyChanged();
            }
        }
        private int activeCheckInterval = 50;
        public int ActiveCheckInterval
        {
            get => activeCheckInterval; set
            {
                if(value >= 0 && value < 1)
                {
                    value = 1;
                }
                activeCheckInterval = value;
                OnPropertyChanged();
            }
        }
        private bool isCheckShiftKeyPress = false;
        public bool IsCheckShiftKeyPress
        {
            get => isCheckShiftKeyPress; set
            {
                isCheckShiftKeyPress = value;
                OnPropertyChanged();
            }
        }

        private int fontSize = 24;
        public int FontSize {
            get => fontSize; set
            {
                if (value < 0)
                {
                    value = 20;
                }
                fontSize = value;
                OnPropertyChanged();
            }
        }

        public static void Save(AppConfig appConfig)
        {
            // save appConfig to json
            var json = JsonSerializer.Serialize(appConfig);
            System.IO.File.WriteAllText("config.json", json);
        }

        public static AppConfig Load()
        {
            // load appConfig from json
            if (System.IO.File.Exists("config.json"))
            {
                var json = System.IO.File.ReadAllText("config.json");
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            else
            {
                return new AppConfig();
            }
        }
    }
}
