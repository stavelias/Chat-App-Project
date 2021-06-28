using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using static ChatServer.Message;

namespace ChatServer
{
	public class SqlQueries
	{
		public static QueryResult SendQuery(MessageType messageType, string query)
		{
			if (!File.Exists("Server.db")) CreateDatabase();

			var connection = new SqliteConnection("Data Source=Server.db");
			connection.Open();

			var command = connection.CreateCommand();
			command.CommandText = query;

			QueryResult queryResult = QueryResult.Ok;

			using (var reader = command.ExecuteReader())
			{
				switch (messageType)
				{
					case MessageType.LoginRequest:
						if (reader.HasRows) queryResult = QueryResult.LoginSuccessful;
						else queryResult = QueryResult.IncorrectDetails;
						break;
					case MessageType.RegisterationRequest:
						if (reader.RecordsAffected == ROWS_AFFECTED) queryResult = QueryResult.RegisterationSuccessful;
						else queryResult = QueryResult.UsernameAlreadyExists;
						break;
					default:
						break;
				}
			}

			return queryResult;
		}

        public static void CreateDatabase()
		{
			var connection = new SqliteConnection("Data Source=Server.db");
			connection.Open();

			var command = connection.CreateCommand();
			command.CommandText = 
			@"
				CREATE TABLE Users (
				username TEXT,
				password TEXT,
				UNIQUE(username)
				);
			";

			command.ExecuteReader();
		}

		public enum QueryResult
		{
			Ok = 0,
			LoginSuccessful = 1,
			RegisterationSuccessful = 2,
			IncorrectDetails = 3,
			UsernameAlreadyExists = 4
		}

		const int ROWS_AFFECTED = 1;
	}
}
