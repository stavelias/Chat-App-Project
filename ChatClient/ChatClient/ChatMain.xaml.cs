using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
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
using static ChatClient.Message;

namespace ChatClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class ChatMain : Window
	{
        Client chatClient;

        // Consumers register to receive data.

        public ChatMain(string IP, int port, string clientName)
		{
			InitializeComponent();
            chatClient = new Client(IP, port, this, clientName);
        }

        #region Send Message Functions

        private void SendMsgBtnClick(object sender, MouseButtonEventArgs e)
        {
            SendMessageToServer(MessageType.Regular, chatClient._clientName, messageBox.Text, channelList.Text);
            messageBox.Text = "";
        }

        private void SendMessageToServer(MessageType type, string clientName, string message, string channel)
        {
            // Sending the data to the server
            chatClient.SendData(Message.CreateMessage(type, clientName, message, channelList.Text));
        }

        #endregion

        #region Channel Updater

        public void UpdateChannels(Message serverMessage)
        {
            // Intializing the channels array
            string[] channels = serverMessage.MessageText.Split(", ");

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Clearing the channel list
                channelList.Items.Clear();

                // Adding the new channels array to the channel list
                AddItemsToChannelList(channels);
            });
        }

        public void AddItemsToChannelList(string[] channels)
        {
            foreach (string channel in channels)
            {
                // Adding the channels to the channel selector
                ComboBoxItem newChannel = new ComboBoxItem();
                newChannel.Content = channel;
                channelList.Items.Add(newChannel);
            }

            // Selecting the Default Channel as the connected channel
            channelList.SelectedIndex = 0;
        }

        private void ChannelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Event Triggered when client requests to move a channel
            if (chatClient != null && channelList.SelectedValue != null)
            {
                // Gets the old and new channel
                string oldChannel = channelList.Text;
                string newChannel = channelList.SelectedValue.ToString();

                // If old channel isn't the same as the new, send a move request 
                if (oldChannel != newChannel)
                    SendMessageToServer(MessageType.MoveChannel, chatClient._clientName, newChannel, oldChannel);
            }
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

        private void MinimizeBtnClick(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        #endregion

        public void ChatWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SendMessageToServer(MessageType.Disconnect, chatClient._clientName, "", channelList.Text);
        }
    }
}
