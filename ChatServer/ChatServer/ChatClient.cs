using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
	public class ChatClient
	{
        public ChatClient(TcpClient tcpClient)
        {
			this.TcpClient = tcpClient ?? throw new ArgumentNullException("TcpClient");
        }

        public TcpClient TcpClient { get; private set; }

        public string Channel { get; set; }
    }
}
