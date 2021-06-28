using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using static ChatClient.Message;

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
                            Notice noticeWindow = new Notice();
                            noticeWindow.ok.Click += noticeWindow.ExitApp;
                            noticeWindow.message.Text = " The Server has been disconnected, the client will now close.";
                            noticeWindow.Show();

                            // Unsubscribing from the event, so it wont try to send a disconnection while the connection is closed
                            ChatClientWindow.Closing -= ChatClientWindow.ChatWindow_Closing;

                            // Closing the main window
                            ChatClientWindow.Close();
                        });                     
					}

                    if(isClientConnected)
					{
                        receivedMessage = new Message(utf8.GetString(buffer, 0, numOfBytes));

                        MessageType messageType = receivedMessage.Type;
                        switch (messageType)
                        {
                            case MessageType.ChannelsUpdate:
                                ChatClientWindow.UpdateChannels(receivedMessage);
                                break;
                            case MessageType.LoginRequest:
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    string noticeMessage = "";
                                    if (receivedMessage.Result == QueryResult.LoginSuccessful)
                                    {
                                        ChatClientWindow.channelList.SelectionChanged += ChatClientWindow.ChannelList_SelectionChanged;
                                        ChatClientWindow.ConnectWindow.Close();
                                        ChatClientWindow.Show();
                                        noticeMessage = $"Welcome {receivedMessage.ClientName}.";
                                    }
                                    else
                                    {
                                        noticeMessage = "Login Failed, Username or password are incorrect";
                                    }

                                    Notice noticeWindow = new Notice();
                                    noticeWindow.ChatClientWindow = ChatClientWindow;
                                    noticeWindow.ok.Click += noticeWindow.ServerQueriesResult;
                                    noticeWindow.message.Text = noticeMessage;
                                    noticeWindow.Show();
                                });
                                break;
                            case MessageType.RegisterationRequest:
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    string noticeMessage = "";
                                    if (receivedMessage.Result == QueryResult.RegisterationSuccessful)
                                    {
                                        ChatClientWindow.ConnectWindow.loginTab.IsSelected = true;
                                        noticeMessage = $"Registeration Successful {receivedMessage.ClientName}, You can now login with your user.";                        
                                    }
                                    else
                                    {
                                        noticeMessage = "Registration Failed, Username isn't available";
                                    }

                                    Notice noticeWindow = new Notice();
                                    noticeWindow.ChatClientWindow = ChatClientWindow;
                                    noticeWindow.ok.Click += noticeWindow.ServerQueriesResult;
                                    noticeWindow.message.Text = noticeMessage;
                                    noticeWindow.Show();
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

            private NetworkStream _stream;
            private Thread _thread;
            public ChatMain ChatClientWindow;
        }
    }
}
