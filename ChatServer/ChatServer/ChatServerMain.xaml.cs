using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatServer
{
	public partial class ChatServerMain : Window
	{
        Server chatServer;
        const string IP = "127.0.0.1";
        const int PORT = 5000;

        public ChatServerMain()
		{
            InitializeComponent();
            chatServer = new Server(this, IP, PORT);
        }

        async void StartServerBtnClick(object sender, RoutedEventArgs e)
		{
            await Task.Run(() =>
            {
            Application.Current.Dispatcher.Invoke(() => {
                messages.AppendText($"Server has been started on IP: {IP} And Port: {PORT.ToString()}\nListening...\n");
            });    
                chatServer.AcceptClients();
            });
        }

		private void AddChannelBtnClick(object sender, RoutedEventArgs e)
		{
            chatServer.UpdateChannels(null, channel.Text);
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

        private void MinimizeBtnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

		#endregion
	}
}
