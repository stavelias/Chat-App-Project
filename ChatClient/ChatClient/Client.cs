using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using static ChatClient.Message;

namespace ChatClient
{
    public sealed partial class Client
    {
        // Called by producers to send data over the socket.
        public void SendData(string data)
        {
            Encoding utf8 = Encoding.UTF8;
            byte[] bytesToSend = utf8.GetBytes(data);
            _sender.SendData(bytesToSend);
        }

        // Consumers register to receive data.
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public Client(string IP, int port, ChatMain ChatClient)
        {
            ChatClientWindow = ChatClient;           
			try
			{
                _client = new TcpClient(IP, port);
                _stream = _client.GetStream();

                _receiver = new Receiver(_stream, ChatClient);
                _sender = new Sender(_stream, ChatClient);

                _receiver.DataReceived += OnDataReceived;

                // Informs the Server that this client has been connected
                SendData(Message.CreateMessage(MessageType.Connect, _clientName, "", "General Channel"));
            }
            catch(Exception ex)
			{
                // Creating the notice message for the client user
                Notice noticeWindow = new Notice();
                noticeWindow.message.Text = " The Server is offline, the client will now close.";
                noticeWindow.Show();

                // Unsubscribing from the event, so it wont try to send a disconnection while the connection is closed
                ChatClientWindow.Closing -= ChatClientWindow.ChatWindow_Closing;

                // Closing the main window
                ChatClientWindow.Close();
            }
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            var handler = DataReceived;
            if (handler != null) DataReceived(this, e);  // re-raise event
        }

        private TcpClient _client;
        private NetworkStream _stream;
        private Receiver _receiver;
        private Sender _sender;
        public string _clientName;
        public ChatMain ChatClientWindow;
    }
}
