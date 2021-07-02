using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        

        // Consumers register to receive data.

        public ChatMain()
		{
            InitializeComponent();
        }

        public void ConnectToServer()
		{
            chatClient = new Client(IP, port, this);
        }

        #region Send Message Functions

        private void SendMsgBtnClick(object sender, MouseButtonEventArgs e)
        {
            SendMessageToServer(MessageType.Regular, chatClient._clientName, messageBox.Text);
            messageBox.Text = "";
        }

        public void SendMessageToServer(MessageType type, string clientName, string message)
        {
            // Sending the data to the server
            if(authCompelete)
			{
                chatClient.SendData(Message.CreateMessage(type, clientName, message, currentChannel));
            }
            else
			{
                chatClient.SendData(Message.CreateMessage(type, clientName, message, "NewConnection"));
                authCompelete = true;
            }
        }

        public void SendRequestToServer(string request)
        {
            // Sending the data to the server
            chatClient.SendData(request);
        }

        #endregion

        #region Channel Updater

        public void UpdateChannels(Message serverMessage)
        {
            // Clearing categories
            categories = new List<KeyValuePair<string, List<string>>>();
            
            // Intializing the channels array
            string[] channels = serverMessage.MessageText.Split('|');
            
            // Iterating over all the categories and channels string
            for(int i = 0; i < channels.Length; i++)
			{
                // Even number - Category Name
                if(i % 2 == 0)
				{
                    // Gets the index of the current category's channels
                    int channelsIndex = i + 1;

                    // if the index is out of boundaries, don't access it
                    if (channelsIndex < channels.Length)
					{
                        // add each channel that is related to the category
                        foreach(string channelName in channels[channelsIndex].Split(", "))
						{
                            AddChannelToList(channels[i], channelName);
                        }
                    }
				}
			}

            // Creating the actual TreeView
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Clears all the data from the old categories tree view (the tree view before the update)
                categoriesTreeView.Items.Clear();

                // Creates a list for the channels
                List<string> channelsList = new List<string>();

				// Iterating for each category on the list of categories
				foreach (KeyValuePair<string, List<string>> category in categories)
                {
                    // Creating the category item
                    TreeViewItem categoryItem = new TreeViewItem
                    {
                        Header = category.Key
                    };

                    // Categories will always be expanded
                    categoryItem.IsExpanded = true;

                    // iterate for each channel that is in the same category
                    foreach (string channel in category.Value)
                    {
                        TreeViewItem channelItem = new TreeViewItem();
                        
                        // If the channel that is currently been added is the channel that the client
                        // is connected to, select it on the tree view, if not select the default channel
                        // until the channel that the client is connected to is found or not.

                        if (channel == currentChannel)
						{
                            channelItem.IsSelected = true;
                        }
                        else if(channel == DEFAULT_CHANNEL)
						{
                            channelItem.IsSelected = true;
						}

                        // Sets the name of the channel to the tree view item
                        channelItem.Header = channel;

                        // Adds the channel to the category
                        categoryItem.Items.Add(channelItem);
                        
                        // Adds the channel to the channel list
                        channelsList.Add(channel);
                    }

                    // Adds the category to the tree view
                    categoriesTreeView.Items.Add(categoryItem);
                }

                // Sets the new channel source, as the channel list created
                channelArr = channelsList.ToArray();
            });
        }

        public void CategoriesTreeView_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // If channel is changed, send a move channel request to the server
            SendMoveChannelRequest();
        }

        public void AddChannelToList(string category, string channelName)
        {
            // If category exists, add it to the specific category
            if (CategoryExists(category, channelName))
            {
                foreach (KeyValuePair<string, List<string>> categ in categories)
                {
                    if (categ.Key == category)
                    {
                        categ.Value.Add(channelName);
                    }
                }
            }
        }

        public bool CategoryExists(string categoryName, string channel)
        {
            // Searches for the category
            foreach (KeyValuePair<string, List<string>> category in categories)
            {
                // if found, return true and the channel will be added
                if (category.Key == categoryName)
                    return true;
            }

            // if not, create the category and add the channel to it
            categories.Add(new KeyValuePair<string, List<string>>(categoryName, new List<string> { channel }));

            // returns false so the channel won't be added twice
            return false;
        }

        public void SendMoveChannelRequest()
		{
            if (chatClient != null && categoriesTreeView.SelectedItem != null)
            {
                // Gets the old and new channel
                string newChannel = categoriesTreeView.SelectedValue.ToString();
                string moveChannelReqStr = $"{currentChannel} {newChannel}";

                // check if it's a channel and not a category
                if (!channelArr.Contains(newChannel)) return;

                // Avoiding sending the same request twice
                if (PreviousMoveChannelRequest == moveChannelReqStr) return;
                PreviousMoveChannelRequest = moveChannelReqStr;


                // If old channel isn't the same as the new, send a move request 
                if (currentChannel != newChannel)
				{
                    SendMessageToServer(MessageType.MoveChannel, chatClient._clientName, newChannel);
                    currentChannel = newChannel;
                }
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
            // When the window is closing, send a disconnection request to the server
            SendMessageToServer(MessageType.Disconnect, chatClient._clientName, "");
        }

        // Initializing a default channel, so both the client and the server
        // knows where the client is connected to on first login
        public const string DEFAULT_CHANNEL = "General Channel";

        // Initializing this to notice the server when the *user* logged
        // in to the channel, rather than the *client* itself.
        public bool authCompelete = false;

        // Saving the previous move channel request message, to avoid sending
        // the same request twice
        public string PreviousMoveChannelRequest;
        
        // Initializing the connect window, so when the user logs in, it will destroy
        // itself, without closing the application
        public Connect ConnectWindow;

        // Category List with its channels
        public List<KeyValuePair<string, List<string>>> categories = new List<KeyValuePair<string, List<string>>>();

        // Channels only list
        string[] channelArr;

        // Current channel
        string currentChannel;

        // Checks if the client is connected
        public bool isClientConnected = false;

        // Client instance, and details
        public Client chatClient;
        public string IP;
        public int port;
	}
}
