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
		public Notice(ChatMain chatMain, NoticeFunctions function, string noticeMessage)
		{
			InitializeComponent();

			// If Main Chat Window is given, Initialize it.
			if(chatMain != null) ChatClientWindow = chatMain;

			// Sets the notice message
			message.Text = noticeMessage;

			// Sets the function wanted for the message
			switch (function)
			{
				case NoticeFunctions.ServerQueriesResult:
					ok.Click += ServerQueriesResult;
					break;
				case NoticeFunctions.ExitApp:
					ok.Click += ExitApp;
					break;
				default:
					ok.Click += OKBtnClick;
					break;
			}

			this.Show();
		}

		private void OKBtnClick(object sender, RoutedEventArgs e)
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

		public enum NoticeFunctions
		{
			ServerQueriesResult = 0,
			ExitApp = 1,
			OkBtnClick = 2
		}

		public ChatMain ChatClientWindow;
	}
}
