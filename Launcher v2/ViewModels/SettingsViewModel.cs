using Launcher.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.ViewModels
{
    class SettingsViewModel : NotifyPropertyChanged
    {
        private OpenFileDialogVM _openFile;
        private MainViewModel _main;
        private Configuration _config;

        public string Path
        {
            get
            {
                return _config.AppSettings.Settings["Path"].Value;
            }
            set
            {
                
                _config.AppSettings.Settings["Path"].Value = value;
                _config.Save();
                OnPropertyChanged("Path");
            }
        }
        public List<string> SHA { get; set; }
        public bool SkipIntro
        {
            get
            {
                try
                {

                    return _config.AppSettings.Settings["SkipIntro"].Value == "true" ? true : false;
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    _config.AppSettings.Settings["SkipIntro"].Value = "true";
                }
                else
                {
                    _config.AppSettings.Settings["SkipIntro"].Value = "false";
                }
                _config.Save();
                OnPropertyChanged("SkipIntro");
            }
        }
        public bool MultiInstance
        {
            get
            {
                try
                {
                    return _config.AppSettings.Settings["MultiInstance"].Value == "true" ? true : false;
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    _config.AppSettings.Settings["MultiInstance"].Value = "true";
                }
                else
                {
                    _config.AppSettings.Settings["MultiInstance"].Value = "false";
                }
                _config.Save();
                OnPropertyChanged("MultiInstance");
            }
        }

        public RelayCommand PathCommand
        {
            get
            {
                return new RelayCommand(o =>
                {
                    OpenFileDialog();
                },
                o => true);
            }
        }

        private void OpenFileDialog()
        {

            var path = Path;
            if (path == "") path = @"C:\Program Files (x86)\KRU\Dark Ages\";
            _openFile = new OpenFileDialogVM(path);
            _openFile.ExecuteOpenFileDialog();
            Path = _openFile.SelectedPath;
        }

        public RelayCommand BackCommand
        {
            get
            {
                return new RelayCommand(o =>
               {
                   ReturnToMain();
               },
                o => true);
            }
        }

        private void ReturnToMain()
        {
            ((MainWindow)App.Current.MainWindow).frame.Navigate(new MainView(_main));
        }

        public SettingsViewModel(MainViewModel main)
        {
            _main = main;
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
    }
}
