using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;
using static ChatClient.Message;

namespace ChatClient
{
	/// <summary>
	/// Interaction logic for Connect.xaml
	/// </summary>
	public partial class Connect : Window
	{
		public Connect()
		{
			InitializeComponent();
			
			//Variable Initialization
			chatMain = new ChatMain();
			isClientConnected = false;

			// Reading Connection Settings
			ReadConfig();
		}



		public void ConnectClient()
		{
			// Connects the client to the server
			chatMain.ConnectToServer();
			isClientConnected = true;
		}

		public void ReadConfig()
		{
			var configFile = new IniFile("config.ini");
			const string DEFAULT_IP = "127.0.0.1";
			const int DEFAULT_PORT = 5000;

			// Checks if config file exists, if not create one
			if (!File.Exists("config.ini"))
			{
				configFile.Write("IP", DEFAULT_IP);
				configFile.Write("Port", DEFAULT_PORT.ToString());

				// Sets Default Values to the local variables
				chatMain.IP = DEFAULT_IP;
				chatMain.port = DEFAULT_PORT;
			}
			else
			{
				// Checks if keys are exists for each key, if not make them
				if (configFile.KeyExists("IP"))
					chatMain.IP = configFile.Read("IP");
				else
					configFile.Write("IP", DEFAULT_IP);

				if (configFile.KeyExists("Port"))
					chatMain.port = Int32.Parse(configFile.Read("Port"));
				else
					configFile.Write("Port", DEFAULT_PORT.ToString());
			}

			ip.Text = chatMain.IP;
			ip.SelectionStart = ip.Text.Length;
			port.Text = chatMain.port.ToString();
			port.SelectionStart = port.Text.Length;
		}

		#region UI Button Functions

		private void UpdateConfigBtnClick(object sender, MouseButtonEventArgs e)
		{
			var configFile = new IniFile("config.ini");
			configFile.Write("IP", ip.Text);
			configFile.Write("Port", port.Text.ToString());

			chatMain.IP = ip.Text;
			chatMain.port = Int32.Parse(port.Text);
		}

		private void LoginBtnClick(object sender, MouseButtonEventArgs e)
		{
			byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(pwdLogin.Text);
			string encryptedPwd = System.Convert.ToBase64String(pwdBytes);

			// Sending the login request to the server
			chatMain.SendRequestToServer(CreateAuthRequest(MessageType.LoginRequest, unLogin.Text, encryptedPwd));
		}

		private void RegisterBtnClick(object sender, MouseButtonEventArgs e)
		{
			byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(pwdLogin.Text);
			string encryptedPwd = System.Convert.ToBase64String(pwdBytes);

			// Sending the login request to the server
			chatMain.SendRequestToServer(CreateAuthRequest(MessageType.RegisterationRequest, unLogin.Text, encryptedPwd));
		}

		#endregion

		#region TopTaskbar Functions

		private void TopTaskbarClick(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void ExitBtnClick(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}
		#endregion

		public ChatMain chatMain;
		public bool isClientConnected;
	}
}
