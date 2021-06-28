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
	/// Interaction logic for Notice.xaml
	/// </summary>
	public partial class Notice : Window
	{
		public Notice()
		{
			InitializeComponent();
		}

		private void OKBtnClick(object sender, MouseButtonEventArgs e)
		{
			this.Close();
		}

		public void ExitApp(object sender, RoutedEventArgs e)
		{
			Environment.Exit(1);
		}

		public void ServerQueriesResult(object sender, RoutedEventArgs e)
		{
			ChatClientWindow.Show();
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

		public ChatMain ChatClientWindow;
	}
}
