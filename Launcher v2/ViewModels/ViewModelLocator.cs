using System.ComponentModel;
using System.Windows.Controls;

namespace Launcher.ViewModels
{
    public class ViewModelLocator
    {
        private static bool? _isInDesignMode;
        private MainViewModel _runtimeMainViewModel;
        private MainViewModel _designtimeMainViewModel;
        private SettingsViewModel _settingsViewModel;
        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    _isInDesignMode = false; //revisit
                }
                return _isInDesignMode.Value;
            }
        }
        protected MainViewModel RuntimeMainViewModel
        {
            get
            {
                // only allow a single instance of the viewmodel constructed per view
                if (_runtimeMainViewModel == null)
                {
                    RuntimeMainViewModel = new MainViewModel(new Frame());
                }
                return _runtimeMainViewModel;
            }
            set
            {
                _runtimeMainViewModel = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MainViewModel"));
            }
        }

        public MainViewModel MainViewModel => IsInDesignMode ? DesigntimeMainViewModel : RuntimeMainViewModel;

        public MainViewModel DesigntimeMainViewModel
        {
            get
            {
                // in our case, the design time view model is the same class as the 
                // runtime view model
                return _designtimeMainViewModel ?? (_designtimeMainViewModel = new MainViewModel(new Frame()));
            }
            set
            {
                _designtimeMainViewModel = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MainViewModel"));
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                if (_settingsViewModel == null)
                {
                    SettingsViewModel = new SettingsViewModel(MainViewModel);
                }
                return _settingsViewModel;
            }
            set
            {
                _settingsViewModel = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SettingsViewModel"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
