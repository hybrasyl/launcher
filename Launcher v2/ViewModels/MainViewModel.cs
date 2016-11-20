using Launcher.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Launcher.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        public Frame F { get; set; }
        private string _serverStatus;
        private Configuration _config;
        private KeyValuePair<string, KeyValuePair<string, int>> _selectedServer;

        public Dictionary<string, KeyValuePair<string, int>> ServerList { get; set; }

        public KeyValuePair<string, KeyValuePair<string, int>> SelectedServer
        {
            get
            {
                if (_selectedServer.Key == null)
                {
                    SelectedServer = ServerList.FirstOrDefault(o => o.Key == _config.AppSettings.Settings["Server"].Value);
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

        public Page NewsPage => new News();

        public Brush ServerStatusColor
        {
            get
            {
                switch (ServerStatus)
                {
                    case "Online":
                        return Brushes.Lime;
                    case "Offline":
                        return Brushes.Red;
                    case "Unknown":
                        return new SolidColorBrush(Color.FromArgb(0XFF, 0xFF, 0xCC, 0x66));
                }
                ServerStatus = "Unknown";
                return new SolidColorBrush(Color.FromArgb(0XFF, 0xFF, 0xCC, 0x66));
            }
        }

        public string NotifyHeader
        {
            get
            {
                return "Notice: Settings Bug";
            }
            set
            {
                _serverStatus = value;
            }
        }

        public string NotifyContent
        {
            get
            {
                return "There is a known issue when selecting a server after modifying settings in the launcher. Please select a server prior to changing settings. We're working on a fix as fast as possible.";
            }
            set
            {
                _serverStatus = value;
            }
        }

        public Visibility NotifyVisibility
        {
            get
            {
                return Visibility.Visible; //for later use.
            }
        }

        public bool LaunchEnabled { get; set; }

        public string HostName { get; set; }
        public int Port { get; set; }

        public MainViewModel(Frame f)
        {
            F = f;
            //f.Navigate(new News());
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ServerList = new Dictionary<string, KeyValuePair<string, int>>
            {
                {
                    "Hybrasyl Production",
                    new KeyValuePair<string, int>("prd.hyb.onl", 2610)
                },
                {
                    "Hybrasyl Staging",
                    new KeyValuePair<string, int>("stg.hyb.onl", 2610)
                },
                {"localhost", new KeyValuePair<string, int>("127.0.0.1", 2610)},
                {"commercial", new KeyValuePair<string, int>("da0.kru.com", 2610) }
            };
            LaunchEnabled = true;
            OnPropertyChanged("LaunchEnabled");
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
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string path = _config.AppSettings.Settings["Path"].Value;

            if (!File.Exists(path))
            {
                MessageBox.Show("Hybrasyl Launcher could not find Dark Ages located at: \r\n" + path, "File not found!", MessageBoxButton.OK, MessageBoxImage.Hand);
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
                    stream.Position = 0x42E625L; // intro
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                    stream.WriteByte(0x90);
                }

                #region Legacy IPAddress Code
                /*Legacy IP Address code. do not remove.*/
                //stream.Position = 0x4333C2L; // ip
                //IPAddress addr = Dns.GetHostAddresses(HostName)[0];
                //var serverBytes = addr.GetAddressBytes();
                //stream.WriteByte(0x6A);
                //stream.WriteByte(serverBytes[3]);
                //stream.WriteByte(0x6A);
                //stream.WriteByte(serverBytes[2]);
                //stream.WriteByte(0x6A);
                //stream.WriteByte(serverBytes[1]);
                //stream.WriteByte(0x6A);
                //stream.WriteByte(serverBytes[0]);

                //stream.Position = 0x4333F5L; // port
                //stream.WriteByte((byte)(Port % 256));
                //stream.WriteByte((byte)(Port / 256));
                /*End legacy IP Address code. do not remove.*/
                #endregion

                var hostBytes = Encoding.UTF8.GetBytes(HostName);
                byte[] endBytes = new byte[12];
                if (hostBytes.Length != 12)
                {
                    for (var i = 0; i < hostBytes.Length; i++)
                    {
                        endBytes[i] = hostBytes[i];
                    }
                }
                else
                {
                    endBytes = hostBytes;
                }

                stream.Position = 0x6707A8L;
                stream.Write(endBytes, 0, 12);

                if (_config.AppSettings.Settings["MultiInstance"].Value == "true")
                {
                    stream.Position = 0x57A7D9L; // multi-instance
                    stream.WriteByte(0xEB);
                }
                else
                {
                    LaunchEnabled = false;
                    OnPropertyChanged("LaunchEnabled");
                }
            }
            Kernel32.ResumeThread(information.ThreadHandle);
            var process = Process.GetProcessById(information.ProcessId);
            process.WaitForInputIdle();

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
            ((MainWindow)Application.Current.MainWindow).frame.Navigate(new Settings(this));
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
                    try
                    {
                        var ar = tcp.BeginConnect(HostName, Port, null, null);
                        if (!ar.AsyncWaitHandle.WaitOne(1000, true))
                        {
                            //tcp.EndConnect(ar);
                            tcp.Close();
                            throw new SocketException();
                        }
                        tcp.EndConnect(ar);

                        await Task.Delay(100);
                        return "Online";
                    }
                    catch (ObjectDisposedException)
                    {
                        return "Offline";
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut || ex.SocketErrorCode == SocketError.NotConnected)
                        {
                            return "Offline";
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut || ex.SocketErrorCode == SocketError.NotConnected)
                {
                    return "Offline";
                }
            }
            catch (ObjectDisposedException)
            {
                return "Offline";
            }

            return "Unknown";
        }
    }
}