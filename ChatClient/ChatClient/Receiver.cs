using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using static ChatClient.Message;
using static ChatClient.Notice;

namespace ChatClient
{
    public sealed partial class Client
    {

		private sealed class Receiver
        {
            internal event EventHandler<DataReceivedEventArgs> DataReceived;

            internal Receiver(NetworkStream stream, ChatMain ChatClient)
            {
                ChatClientWindow = ChatClient;
                _stream = stream;
                _thread = new Thread(Run);
                _thread.Start();
            }

            private void Run()
            {
                // Setting Encoding to UTF8 To Read Hebrew Characters
                Encoding utf8 = Encoding.UTF8;

                // Read Buffer Initialization
                byte[] buffer = new byte[1024];

				// Determines how many bytes has been received
				int numOfBytes = 0;

                // Checking if the client is still connected to the server
                bool isClientConnected = true;

                // Received Data will be stored here
                Message receivedMessage;

                // Receiving Data
                while (isClientConnected)
				{
					try
					{
                        numOfBytes = _stream.Read(buffer, 0, 1024);
                    }
                    catch(Exception ex)
					{
                        // Noticing the client about the disconnection
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            isClientConnected = false;

                            // Creating the notice message for the client user
                            string noticeMessage = "The Server has been disconnected, the client will now close.";
                            Notice noticeWindow = new Notice(null, NoticeFunctions.ExitApp, noticeMessage);

                            // Unsubscribing from the event, so it wont try to send a disconnection while the connection is closed
                            ChatClientWindow.Closing -= ChatClientWindow.ChatWindow_Closing;

                            // Closing the main window
                            ChatClientWindow.Close();
                        });                     
					}

                    if(isClientConnected)
					{
                        receivedMessage = new Message(utf8.GetString(buffer, 0, numOfBytes));

                        // Getting the message type fron the recieved message
                        MessageType messageType = receivedMessage.Type;

                        // Acts according to the message type
                        switch (messageType)
                        {
                            case MessageType.ChannelsUpdate:
                                // Update Channels on the main window
                                ChatClientWindow.UpdateChannels(receivedMessage);
                                break;
                            case MessageType.LoginRequest:
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    string noticeMessage = "";

                                    // Initialize a notice message according to the login result
                                    if (receivedMessage.Result == QueryResult.LoginSuccessful)
                                    {
                                        LoginSuccessful();
                                        noticeMessage = $"Welcome {receivedMessage.ClientName}.";
                                    }
                                    else
                                    {
                                        noticeMessage = "Login Failed, Username or password are incorrect";
                                    }

                                    // Show a notice with the message for the user
                                    Notice noticeWindow = new Notice(ChatClientWindow, NoticeFunctions.ServerQueriesResult, noticeMessage);
                                });
                                break;
                            case MessageType.RegisterationRequest:
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    string noticeMessage = "";

                                    // Initialize a notice message according to the login result
                                    if (receivedMessage.Result == QueryResult.RegisterationSuccessful)
                                    {
                                        ChatClientWindow.ConnectWindow.loginTab.IsSelected = true;
                                        noticeMessage = $"Registeration Successful {receivedMessage.ClientName}, You can now login with your user.";                        
                                    }
                                    else
                                    {
                                        noticeMessage = "Registration Failed, Username isn't available";
                                    }

                                    // Show a notice with the message for the user
                                    Notice noticeWindow = new Notice(ChatClientWindow, NoticeFunctions.ServerQueriesResult, noticeMessage);
                                });
                                break;
                            default:
                                AddToMessages(receivedMessage.PublicMessage);
                                break;
                        }
                    }
                }
                
            }

            public void AddToMessages(string message)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Append the data given to the UI TextBox
                    ChatClientWindow.messages.AppendText("\n" + message);
                    
                    /* Scrolls the UI TextBox to the end, so it will always display the
                       last message that has been received */
                    ChatClientWindow.messages.ScrollToEnd();
                });
            }

            public void LoginSuccessful()
			{
                // Login was successful
                // Preparing events, and opening the main window
                ChatClientWindow.channelList.SelectionChanged += ChatClientWindow.ChannelList_SelectionChanged;
                ChatClientWindow.ConnectWindow.Close();
                ChatClientWindow.Show();
            }

            private NetworkStream _stream;
            private Thread _thread;
            public ChatMain ChatClientWindow;
        }
    }
}
