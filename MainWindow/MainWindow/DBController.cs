﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace MainWindow
{
    public static class DBController
    {
        private static readonly string DB_FILE = "telegram.sqlite";
        private static SQLiteConnection m_dbConnection = null;
        public static void Init()
        {
            if (m_dbConnection == null)
            {
                if (!System.IO.File.Exists(DB_FILE))
                {
                    SQLiteConnection.CreateFile(DB_FILE);
                }
                m_dbConnection = new SQLiteConnection("Data Source=" + DB_FILE + "; Version=3;");
                m_dbConnection.Open();
            }

            CheckTableExistence("chat");
            CheckTableExistence("user");

            foreach (string supportedCoin in Settings.supportedCoins)
            {
                CheckTableExistence(supportedCoin);
            }

            CheckTableExistence("invest");
        }
        public static bool EntryExist(string query)
        {
            try
            {
                int converted = Convert.ToInt32(ReturnFirst(query));
                return converted > 0;
            }
            catch
            {
                return false;
            }
        }

        public static void Close()
        {
            m_dbConnection.Close();
        }
        public static bool ExecuteQuery(string query)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("(ExecuteQuery) Error: " + ex.Message + Environment.NewLine + "Query: " + Environment.NewLine + query);
                return false;
            }
        }
        public static object ReturnFirst(string query)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
                return command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine("(ReturnFirst) Error: " + ex.Message);
                return null;
            }
        }
        public static SQLiteDataReader ReturnQuery(string query)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine("(ReturnQuery) Error: " + ex.Message);
                return null;
            }
        }
        private static void CheckTableExistence(string tableName)
        {
            if (!TableExist(tableName))
            {
                CreateTable(tableName);
            }
        }
        public static void AddUser(ChatUser user)
        {
            try
            {
                if (!EntryExist("SELECT * FROM user WHERE userID = '" + user._user.Id + "' LIMIT 1"))
                {
                    ExecuteQuery("INSERT INTO user (userID, userDATA) VALUES('" + user._user.Id + "', '" + JsonConvert.SerializeObject(user) + "')");
                }
                else
                {
                    ExecuteQuery("UPDATE user SET userDATA = '" + JsonConvert.SerializeObject(user) + "' WHERE userID = '" + user._user.Id + "'");
                }

            }
            catch
            {

            }
        }
        public static void AddChat(Chat chat)
        {
            try
            {
                ExecuteQuery("INSERT INTO chat (chatID, chatDATA) VALUES('" + chat.Id + "', '" + JsonConvert.SerializeObject(chat) + "')");
            }
            catch
            {

            }
        }
        private static bool TableExist(string tableName)
        {
            return ExecuteQuery("SELECT 1 FROM " + tableName + " LIMIT 1");
        }
        private static void CreateTable(string tableName)
        {
            Console.WriteLine("Creating Table... " + tableName);
            bool found = false;

            switch (tableName)
            {
                case "user":
                    ExecuteQuery("CREATE TABLE `" + tableName.ToLower() + "` (`id` INTEGER PRIMARY KEY, `userID` int(255), `userDATA` VARCHAR(1000) NOT NULL)");
                    found = true;
                    break;
                case "chat":
                    ExecuteQuery("CREATE TABLE `" + tableName.ToLower() + "` (`id` INTEGER PRIMARY KEY, `chatID` int(255), `chatDATA` VARCHAR(1000) NOT NULL)");
                    found = true;
                    break;
                case "invest":
                    ExecuteQuery("CREATE TABLE `" + tableName.ToLower() + "` (`id` INTEGER PRIMARY KEY, `userID` int(255), `amount` int(255) NOT NULL)");
                    found = true;
                    break;
            }

            if (!found)
            {
                foreach (string supportedCoin in Settings.supportedCoins)
                {
                    if (tableName == supportedCoin)
                    {
                        ExecuteQuery("CREATE TABLE `" + tableName.ToLower() + "` (`id` INTEGER PRIMARY KEY, `userID` int(255), `amount` int(255) NOT NULL)");
                    }
                }
            }
        }
    }
}
