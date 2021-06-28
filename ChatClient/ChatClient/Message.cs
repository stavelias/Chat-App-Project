using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ChatClient
{
	public class Message
	{
		public Message(string clientMsg)
		{
			string[] messageFrags = clientMsg.Split(SEPERATOR);
			Type = (MessageType)(Int32.Parse(messageFrags[0]));

			switch (Type)
			{
				case MessageType.LoginRequest:
					Result = (QueryResult)(Int32.Parse(messageFrags[1]));
					ClientName = messageFrags[2];
					break;
				case MessageType.RegisterationRequest:
					Result = (QueryResult)(Int32.Parse(messageFrags[1]));
					ClientName = messageFrags[2];
					break;
				default:
					ClientName = messageFrags[1];
					MessageText = messageFrags[2];
					Channel = messageFrags[3];
					PublicMessage = $"[{ClientName}] {MessageText}";
					break;
			}
		}

		public static string CreateMessage(MessageType type, string clientName, string clientMessage, string channel)
		{
			return $"{(int)type}{SEPERATOR}{clientName}{SEPERATOR}{clientMessage}{SEPERATOR}{channel}";
		}

		public static string CreateAuthRequest(MessageType type, string username, string password)
		{
			return $"{(int)type}{SEPERATOR}{username}{SEPERATOR}{password}";
		}



		public enum MessageType
		{
			Connect = 0,
			Disconnect = 1,
			Regular = 2,
			MoveChannel = 3,
			ChannelsUpdate = 4,
			SystemMessage = 5,
			LoginRequest = 6,
			RegisterationRequest = 7
		}

		public enum QueryResult
		{
			Ok = 0,
			LoginSuccessful = 1,
			RegisterationSuccessful = 2,
			IncorrectDetails = 3,
			UsernameAlreadyExists = 4
		}

		const string SEPERATOR = "~*~";
		public MessageType Type { get; set; }

		public string ClientName { get; private set; }
		public string MessageText { get; private set; }
		public string Channel { get; private set; }

		public QueryResult Result;

		public string PublicMessage;
	}
}
