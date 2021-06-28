using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ChatServer.Message;
using static ChatServer.SqlQueries;

namespace ChatServer
{
	class Server
	{
        public Server(ChatServerMain parent, string IP, int port)
        {
            channels.Add(DEFAULT_CHANNEL);

            _server = new TcpListener(IPAddress.Parse(IP), port);
            _server.Start();

            _isRunning = true;

            mainChatServer = parent;
        }

        public void AcceptClients()
        {
            while (_isRunning)
            {
                // wait for client connection
                TcpClient newClient = _server.AcceptTcpClient();
                ChatClient client = new ChatClient(newClient);
                clients.Add(client);
                
                // client found.
                // create a thread to handle communication
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(client);
            }
        }

        public void HandleClient(object obj)
        {
            // retrieve client from parameter passed to thread
            ChatClient client = (ChatClient)obj;

            // sets the client default channel
            client.Channel = DEFAULT_CHANNEL;

            // Intialization of Connection and Disconnection Messages
            

            // Defining the read buffer
            byte[] buffer = new byte[1024];
            int numOfBytes = 0;

            // Stores the Message received from the client
            Message clientMessage;

            // Defines the message type
            MessageType messageType;

            // Retrieving the stream from the client
            NetworkStream stream = client.TcpClient.GetStream();

            // Determines if the client is connected
            Boolean bClientConnected = true;

            // Defines the encoding as utf8, so we can use Hebrew characters
            Encoding utf8 = Encoding.UTF8;

            // Defines the current channel
            string currentChannel = DEFAULT_CHANNEL;

            // Read while the client is connected  
            while (bClientConnected)
            {
				try
				{
                    // Reads Data from the client
                    if(bClientConnected)
                        numOfBytes = stream.Read(buffer, 0, buffer.Length);

                    // Stores the message received from the client as string
                    clientMessage = new Message(utf8.GetString(buffer, 0, numOfBytes));

                    // Defines the message type receieved by the client
                    messageType = (MessageType)clientMessage.Type;

                    switch(messageType)
					{
                        case MessageType.Connect:

                            

                            // Sends the client the channel list
                            

                            // Cleans the buffer
                            buffer = new byte[1024];
                            break;
                        case MessageType.Disconnect:
                            // Closee connection with the client
                            bClientConnected = false;

                            //Notifies everyone about the disconnection
                            SystemMessage(MessageType.Disconnect, leaveStr, client, currentChannel);
                            break;
                        case MessageType.Regular:
                            // Sends everyone the message
                            SendMessage(null, Message.CreateMessage(MessageType.Regular, clientMessage.ClientName, clientMessage.MessageText, clientMessage.Channel), null, clientMessage.Channel);
                            AddToMessages($"[{clientMessage.Channel}] {clientMessage.PublicMessage}");
                            break;
                        case MessageType.MoveChannel:
                            MoveChannelRequest(clientMessage, client);
                            break;
                        case MessageType.LoginRequest:
                            Login(client, clientMessage.Username, clientMessage.Password, joinStr);
                            break;
                        case MessageType.RegisterationRequest:
                            Register(client, clientMessage.Username, clientMessage.Password);
                            break;
                        default:
                            break;
                    }
                }
                catch(Exception e)
                {
                    if(bClientConnected)
					{
                        // Disconnectes the client
                        bClientConnected = false;
                        SystemMessage(MessageType.Disconnect, leaveStr, client, currentChannel);
                        if(!(e is IOException && e.InnerException is SocketException))
                            AddToMessages($"[{e.GetType()}] {e.Message}");
                    }
                }

            }
        }

        public void AddToMessages(string message)
		{
            // Adds a message to the messages list on the main window UI
            Application.Current.Dispatcher.Invoke(() =>
            {
				mainChatServer.messages.AppendText($"\n{message}");
                mainChatServer.messages.ScrollToEnd();
            });
        }

        #region Send Functions

        public void SendMessage(byte[] dataBytes, string dataString, ChatClient userClient, string channel)
        {
            if (dataBytes == null)
                dataBytes = ASCIIEncoding.ASCII.GetBytes(dataString);

            if (userClient != null) // Send to a specific client
            {
                NetworkStream stream = userClient.TcpClient.GetStream();
                stream.Write(dataBytes);
            }
            else if (channel != null) // Send to a specific channel
            {
                foreach (ChatClient client in clients)
                {
                    if (client.Channel == channel)
                    {
                        NetworkStream stream = client.TcpClient.GetStream();
                        stream.Write(dataBytes);
                    }
                }
            }
            else // Send to Everyone
            {
                // Looping over each client to send the message that has been received
                foreach (ChatClient client in clients)
                {
                    NetworkStream stream = client.TcpClient.GetStream();
                    stream.Write(dataBytes);
                }
            }
        }

        public void SystemMessage(MessageType type, string message, ChatClient client, string currentChannel)
        {
            switch (type)
            {
                case MessageType.Connect:
                    // Informs all connected clients
                    AddToMessages(message);
                    SendMessage(null, Message.CreateMessage(type, "SYSTEM", message, ""), null, null);
                    break;
                case MessageType.Disconnect:
                    // Removed the client from the connected clients list
                    clients.Remove(client);
                    // Closing connection with the client
                    client.TcpClient.Close();
                    // Informs all connected clients
                    AddToMessages(message);
                    SendMessage(null, Message.CreateMessage(type, "SYSTEM", message, ""), null, null);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Server Handlers
        public void MoveChannelRequest(Message moveChannelReq, ChatClient client)
        {
            // Variables Declaration
            /*
                oldChannel - The old channel that the user is moving from
                newChannel - the new channel that the user wants to move to
            */

            string oldChannel = moveChannelReq.Channel;
            string newChannel = moveChannelReq.MessageText;
            // Checks if the move request is valid, and it's a new channel
            if (oldChannel == newChannel || oldChannel == "") return;

            // Changes the channel for this client
            client.Channel = newChannel;

            // Notifies the New Channel about the new client that has joined
            SendMessage(null, Message.CreateMessage(MessageType.SystemMessage, "SYSTEM", moveChannelReq.ClientName + " has joined " + newChannel, newChannel), null, newChannel);
        }

        public void UpdateChannels(ChatClient client, string channelName)
        {
            /*
                True - Channel is given meaning it's an add channel request
                False - A Client has been connected, and requests the channel list
            */
            if (channelName != null)
                channels.Add(channelName);

            // Creates a string from the channels list to be able to send it
            string channelList = String.Join(", ", channels.ToArray());

            // Creates the update string to the client
            string updateString = Message.CreateMessage(MessageType.ChannelsUpdate, "SYSTEM", channelList, "");

            /*  
                True - Client has been given, so its a new client conneciton
                False - A channel has been added, so its an update request
            */
            if (client != null)
                SendMessage(null, updateString, client, null);
            else
                SendMessage(null, updateString, null, null);
        }
		#endregion region

		#region Login System

        public async void Login(ChatClient client, string username, string password, string joinStr)
		{
            string query = $"SELECT username FROM Users WHERE [username]=\'{username}\' AND [password]=\'{password}\'";

            QueryResult queryResult = SendQuery(MessageType.LoginRequest, query);

            NetworkStream stream = client.TcpClient.GetStream();
            string resultMessage = CreateResultMessage(MessageType.LoginRequest, queryResult, username);
            byte[] resultMessageBytes = ASCIIEncoding.ASCII.GetBytes(resultMessage);
            stream.Write(resultMessageBytes);
            
            if(queryResult == QueryResult.LoginSuccessful)
			{
                // Creates the join and leave strings
                joinStr = $"{username} Connected to the server.";
                leaveStr = $"{username} has been Disconnected.";

                // Notifies everyone about the new conneciton
                SystemMessage(MessageType.Connect, joinStr, null, null);
                await Task.Delay(MESSAGE_COOLDOWN);
                UpdateChannels(client, null);
            }
        }

        public void Register(ChatClient client, string username, string password)
		{
            string query = $"INSERT OR IGNORE INTO Users VALUES(\"{username}\", \"{password}\")";
            
            QueryResult queryResult = SendQuery(MessageType.RegisterationRequest, query);

            NetworkStream stream = client.TcpClient.GetStream();
            string resultMessage = CreateResultMessage(MessageType.RegisterationRequest, queryResult, username);
            byte[] resultMessageBytes = ASCIIEncoding.ASCII.GetBytes(resultMessage);
            stream.Write(resultMessageBytes);
        }

		#endregion

		const string DEFAULT_CHANNEL = "General Channel"; 
        private const int MESSAGE_COOLDOWN = 300;
        public string leaveStr = "", joinStr = "";

        private TcpListener _server;
        private Boolean _isRunning;
        public static List<ChatClient> clients = new List<ChatClient>();
        private ChatServerMain mainChatServer;
        public List<string> channels = new List<string>();
    }
}
