/*
 * This file is part of Project Hybrasyl.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the Affero General Public License as published by
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but
 * without ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE. See the Affero General Public License
 * for more details.
 *
 * You should have received a copy of the Affero General Public License along
 * with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * (C) 2013-2015 Kyle Speck (baughj@hybrasyl.com)
 * (C) 2015 Justin Baugh (info@hybrasyl.com)
 *
 * Authors:   Justin Baugh  <baughj@hybrasyl.com>
 *            Kyle Speck    <kojasou@hybrasyl.com>
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;



namespace HybrasylLauncher
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		static string ProgramFilesx86()
		{
				if ( 8 == IntPtr.Size
					 || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
				{
					return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
				}
				return Environment.GetEnvironmentVariable("ProgramFiles");
		}


		private void playButton_Click(object sender, EventArgs e)
		{
			IPAddress server = IPAddress.Parse("169.254.169.254");
			var dialog = new OpenFileDialog();
			dialog.Filter = "*.exe|*.exe";
			dialog.InitialDirectory = string.Format("{0}\\KRU\\Dark Ages", ProgramFilesx86());
			dialog.FileName = "Darkages.exe";
			dialog.Title = "Which Darkages.exe shall I use?";

            try
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        // production
                        server = Dns.GetHostAddresses("production.hybrasyl.com")[0];
                        break;
                    case 1:
                        // testing
                        server = Dns.GetHostAddresses("staging.hybrasyl.com")[0];
                        break;
                    case 2:
                        // localhost (FOR 1337 H4X0RZ)
                        server = Dns.GetHostAddresses("127.0.0.1")[0];
                        break;
                }
            }
            catch (Exception exc)
            {
                // We naively assume any error is due to a resolution failure
                MessageBox.Show("There was an error resolving the hostname of the Hybrasyl servers.", "Hybrasyl Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ProcessInformation information;
				StartupInfo startupInfo = new StartupInfo();
				startupInfo.Size = Marshal.SizeOf(startupInfo);
				Kernel32.CreateProcess(dialog.FileName, null, IntPtr.Zero, IntPtr.Zero, false, ProcessCreationFlags.Suspended,
					IntPtr.Zero, null, ref startupInfo, out information);

				using (ProcessMemoryStream stream = new ProcessMemoryStream(information.ProcessId,
					ProcessAccess.VmWrite | ProcessAccess.VmRead | ProcessAccess.VmOperation))
				{
					stream.Position = 0x4341FAL;
					var bytes = server.GetAddressBytes();
					stream.WriteByte(0x6A);
					stream.WriteByte(bytes[3]);
					stream.WriteByte(0x6A);
					stream.WriteByte(bytes[2]);
					stream.WriteByte(0x6A);
					stream.WriteByte(bytes[1]);
					stream.WriteByte(0x6A);
					stream.WriteByte(bytes[0]);

					stream.Position = 0x434224L;
					stream.WriteByte((byte)(2610 % 256));
					stream.WriteByte((byte)(2610 / 256));
					stream.Position = 0x5911B9L;
					stream.WriteByte(0xEB);

					stream.Position = 0x42F495L;
					stream.WriteByte(0x90);
					stream.WriteByte(0x90);
					stream.WriteByte(0x90);
					stream.WriteByte(0x90);
					stream.WriteByte(0x90);
					stream.WriteByte(0x90);

					Kernel32.ResumeThread(information.ThreadHandle);
				}
			}
		}

		private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{

		}

		private void panel1_Paint(object sender, PaintEventArgs e)
		{

		}


		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			comboBox1.SelectedIndex = 0;
		}

		private void label1_Click(object sender, EventArgs e)
		{

		}
	}
}
