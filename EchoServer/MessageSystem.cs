﻿/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015 Gregory Morgan

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Extensions;
using System;
using System.Data.SqlClient;

namespace EchoServer
{
    public class MessageSystem
    {
        public Message GetNextMessage() // called from server
        {
            // also mark as processing.
            return SQL.GetNextMessage();
        }
    }

    public class Message
    {
        public Guid clientID = Guid.Empty;
        public Guid messageID = Guid.Empty;
        public string textRequest = string.Empty;
        public string textResponse = string.Empty;
        public byte[] audioRequest;
        public byte[] audioResponse = new byte[0];
        public DateTime postTime = new DateTime();
        public enum Status { queued, processing, delayed, ready, closed, error };
        public Status status = Status.queued;
    }

    public class SQL_old
    {
        private static string connectionString = "Server=SKYNET\\SQLEXPRESS;Database=Echo;Trusted_Connection=True;";

        public static Message GetNextMessage()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                Guid msgGuid = GetNextMessageGuid();

                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT [MessageID],[ClientID],[textRequest], " +
                    "[textResponse],[request],[response],[PostTime],[Status]" +
                    " FROM Messages " + "WHERE [MessageID]=@MessageID", con))
                {
                    command.Parameters.Add(new SqlParameter("MessageID", msgGuid));
                    SqlDataReader reader = command.ExecuteReader();

                    Message m = new Message();
                    while (reader.Read())
                    {
                        m.messageID = Guid.Parse(reader["MessageID"].ToString());
                        m.clientID = Guid.Parse(reader["ClientID"].ToString());
                        m.textRequest = reader["textRequest"].ToString();
                        m.textResponse = reader["textResponse"].ToString();
                        m.audioRequest = reader["request"].ToByteArray();
                        m.audioResponse = reader["response"].ToByteArray();
                        m.postTime = reader["PostTime"].ToDateTime();
                        m.status = (Message.Status)reader["Status"].ToInt();

                    }
                    return m;
                }
            }
        }

        public static void UpdateMessage(Message msg)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand command = new SqlCommand(
                    "UPDATE [Messages] " +
                    "SET [textRequest]=@textRequest,[textResponse]=@textResponse,[request]=@request, " +
                    "[response]=@response,[Status]=@Status,[LastUpdated]=GETDATE() " +
                    "WHERE [MessageID]=@MessageID", con))
                {
                    command.Parameters.Add(new SqlParameter("textRequest", msg.textRequest));
                    command.Parameters.Add(new SqlParameter("textResponse", msg.textResponse));
                    command.Parameters.Add(new SqlParameter("request", msg.audioRequest));
                    command.Parameters.Add(new SqlParameter("response", msg.audioResponse));
                    command.Parameters.Add(new SqlParameter("Status", msg.status));
                    command.Parameters.Add(new SqlParameter("MessageID", msg.messageID));
                    command.ExecuteNonQuery();
                }
            }
        }

        public static Guid GetNextMessageGuid()
        {
            Guid msgGuid = Guid.Empty;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("DECLARE @msg uniqueidentifier; " +
                    "SELECT TOP 1 @msg = [MessageID] " +
                    "FROM Messages " +
                    "WHERE [Status] = 0 " +
                    "ORDER BY [PostTime] ASC; " +
                    "SELECT @msg AS [MessageID]; " +
                    "UPDATE [Messages] " +
                    "SET [Status] = 1, [LastUpdated]=GETDATE()" +
                    "WHERE [MessageID] = @msg; ", con))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!string.IsNullOrWhiteSpace(reader["MessageID"].ToString()))
                        {
                            msgGuid = Guid.Parse(reader["MessageID"].ToString());
                        }
                    }
                }
                con.Close();
            }

            return msgGuid;
        }
    }
}
