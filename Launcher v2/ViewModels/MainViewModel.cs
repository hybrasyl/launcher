using Launcher.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Launcher.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private string _serverStatus;
        private Configuration _config;
        private KeyValuePair<string, KeyValuePair<string, int>> _selectedServer;
        public Dictionary<string, KeyValuePair<string, int>> ServerList { get; set; }

        public KeyValuePair<string, KeyValuePair<string, int>> SelectedServer
        {
            get
            {
                if(_selectedServer.Key == null)
                {
                    SelectedServer = ServerList.Where(o => o.Key == _config.AppSettings.Settings["Server"].Value).FirstOrDefault();
                    CheckServer();
                }

                return _selectedServer;
            }
            set
            {
                _selectedServer = value;
                _config.AppSettings.Settings["Server"].Value = _selectedServer.Key;
                _config.Save();
                HostName = _selectedServer.Value.Key;
                Port = _selectedServer.Value.Value;
                OnPropertyChanged("HostName");
                OnPropertyChanged("Port");
                OnPropertyChanged("SelectedServer");
            }
        }
        public string ServerStatus
        {
            get
            {
                return _serverStatus;
            }
            internal set
            {
                _serverStatus = value;
                OnPropertyChanged("ServerStatus");
                OnPropertyChanged("ServerStatusColor");
            }
        }

        public Page NewsPage
        {
            get
            {
                return new News();
            }
        }


        public Brush ServerStatusColor
        {
            get
            {
                if (ServerStatus == "Online") return Brushes.Lime;
                if (ServerStatus == "Offline") return Brushes.Red;
                if (ServerStatus == "Unknown") return new SolidColorBrush(Color.FromArgb(0XFF, 0xFF, 0xCC, 0x66));
                ServerStatus = "Unknown";
                return new SolidColorBrush(Color.FromArgb(0XFF, 0xFF, 0xCC, 0x66));
            }
        }
        public string NotifyHeader
        {
            get
            {
                return "Test Service Alert";
            }
            set
            {

            }
        }
        public string NotifyContent
        {
            get
            {
                return "Test alert Content.";
            }
            set
            {

            }
        }

        public Visibility NotifyVisibility
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public string HostName { get; set; }
        public int Port { get; set; }

        public MainViewModel(Frame f)
        {
            //f.Navigate(new News());
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ServerList = new Dictionary<string, KeyValuePair<string, int>>();
            ServerList.Add("Hybrasyl Production", new KeyValuePair<string, int>(Dns.GetHostAddresses("production.hybrasyl.com")[0].ToString(), 2610));
            ServerList.Add("Hybrasyl Staging", new KeyValuePair<string, int>(Dns.GetHostAddresses("staging.hybrasyl.com")[0].ToString(), 2610));
            ServerList.Add("localhost", new KeyValuePair<string, int>("127.0.0.1", 2610));
            OnPropertyChanged("ServerList");
            
        }

        public RelayCommand PlayCommand
        {
            get
            {
                return new RelayCommand(o => Launch(), o => true);
            }
        }

        private void Launch()
        {
            string path = _config.AppSettings.Settings["Path"].Value;

            if (!File.Exists(path))
            {
                MessageBox.Show("Launcher could not find Dark Ages located at: \r\n" + path, "File not found!", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            ProcessInformation information;
            StartupInfo startupInfo = new StartupInfo();
            startupInfo.Size = Marshal.SizeOf(startupInfo);
            Kernel32.CreateProcess(path, null, IntPtr.Zero, IntPtr.Zero, false, ProcessCreationFlags.Suspended,
                IntPtr.Zero, null, ref startupInfo, out information);

            using (ProcessMemoryStream stream = new ProcessMemoryStream(information.ProcessId,
                ProcessAccess.VmWrite | ProcessAccess.VmRead | ProcessAccess.VmOperation))
            {
                if (_config.AppSettings.Settings["SkipIntro"].Value == "true")
                {
                    stream.Position = 0x42F495L; // intro
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                }

                stream.Position = 0x4341FAL; // ip
                IPAddress addr = Dns.GetHostAddresses(HostName)[0];
                var serverBytes = addr.GetAddressBytes();
                stream.WriteByte(0x6A);
                stream.WriteByte(serverBytes[3]);
                stream.WriteByte(0x6A);
                stream.WriteByte(serverBytes[2]);
                stream.WriteByte(0x6A);
                stream.WriteByte(serverBytes[1]);
                stream.WriteByte(0x6A);
                stream.WriteByte(serverBytes[0]);

                stream.Position = 0x434224L; // port
                stream.WriteByte((byte)(Port % 256));
                stream.WriteByte((byte)(Port / 256));

                if (_config.AppSettings.Settings["MultiInstance"].Value == "true")
                {
                    stream.Position = 0x5911B9L; // multi-instance
                    stream.WriteByte(0xEB);
                }

                Kernel32.ResumeThread(information.ThreadHandle);
            }

            var process = Process.GetProcessById(information.ProcessId);
            process.WaitForInputIdle();

            while (process.MainWindowHandle == IntPtr.Zero) ;

            User32.SetWindowText(process.MainWindowHandle, "DarkAges : Hybrasyl");
        }

        public RelayCommand SettingsCommand
        {
            get
            {
                return new RelayCommand(o => Settings(), o => true);
            }
        }

        private void Settings()
        {
            //i hate this. there's better ways to do this.
            ((MainWindow)App.Current.MainWindow).frame.Navigate(new Settings(this));
        }

        public RelayCommand SelectionChangedCommand
        {
            get
            {
                return new RelayCommand(o => CheckServer(), o => true);
            }
        }


        private async void CheckServer()
        {
           await Task.Run(async () =>
           {
               ServerStatus = await Connect();
           });
            
        }

        private async Task<string> Connect()
        {
            try
            {
                using (TcpClient tcp = new TcpClient())
                {
                    IAsyncResult ar = tcp.BeginConnect(HostName, Port, null, null);
                    try
                    {
                        if (!ar.AsyncWaitHandle.WaitOne(1000, false))
                        {
                            tcp.EndConnect(ar);
                            tcp.Close();
                            throw new SocketException();
                        }
                        tcp.EndConnect(ar);
                    }
                    finally
                    {
                    }
                    await Task.Delay(100);
                    return "Online";
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut)
                {
                    return "Offline";
                }
            }
            return "Unknown";
        }
        
    }
}
