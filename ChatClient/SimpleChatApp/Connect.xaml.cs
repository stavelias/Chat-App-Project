using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
		}

		private void ConnectBtnClick(object sender, MouseButtonEventArgs e)
		{
			ChatMain mainWindow = new ChatMain(IP.Text, Int32.Parse(port.Text), clientName.Text);
			this.Hide();
			mainWindow.Show();
			this.Close();
		}

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
	}
}
