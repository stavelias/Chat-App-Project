using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatClient
{
    public sealed partial class Client
    {
        private sealed class Sender
        {
            internal void SendData(byte[] data)
            {
                // Sends the data to the server over the network stream
                _stream.Write(data);
            }

            internal Sender(NetworkStream stream, ChatMain ChatClient)
            {
                ChatClientWindow = ChatClient;
                _stream = stream;
                _thread = new Thread(Run);
                _thread.Start();
            }

            private void Run()
            {
            }

            private NetworkStream _stream;
            private Thread _thread;
            public ChatMain ChatClientWindow;
        }
    }
}
