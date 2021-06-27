using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ChatServer
{
	public class Message
	{
		public Message(string clientMsg)
		{
			string[] messageFrags = clientMsg.Split(SEPERATOR);
			Type = (MessageType)(Int32.Parse(messageFrags[0]));
			ClientName = messageFrags[1];
			MessageText = messageFrags[2];
			Channel = messageFrags[3];
			PublicMessage = "[" + ClientName + "] " + MessageText;
		}

		public static string CreateMessage(MessageType type, string clientName, string clientMessage, string channel)
		{
			return ((int)type).ToString() + SEPERATOR + clientName + SEPERATOR + clientMessage + SEPERATOR + channel;
		}

		public enum MessageType
		{
			Connect = 0,
			Disconnect = 1,
			Regular = 2,
			MoveChannel = 3,
			ChannelsUpdate = 4,
			SystemMessage = 5
		}

		const string SEPERATOR = "~*~";
		public MessageType Type { get; set; }
		public string ClientName { get; private set; }
		public string MessageText { get; private set; }
		public string Channel { get; private set; }
		public string PublicMessage;
	}
}
