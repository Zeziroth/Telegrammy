using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
namespace MainWindow
{
    class Program
    {
        private static string command = "";
        private static Bot bot = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Initialising...");
            bot = new Bot(Settings.API_KEY);

            while (bot.Data == null) { }

            Console.WriteLine("Connected as: " + bot.Data.Username);
            DBController.Init();
            bot.Init();

            while (command != "end")
            {
                string cmd = command.Contains(' ') ? command.Split(' ')[0] : command;
                List<string> param = command.Split(' ').ToList();
                param.RemoveAt(0);

                switch (cmd.ToLower())
                {
                    case "clear":
                        Console.Clear();
                        break;
                    case "me":
                        Console.WriteLine(bot.Data.FirstName + " " + bot.Data.LastName + " (" + bot.Data.Username + ")");
                        break;
                    case "member":
                    case "members":
                    case "user":
                    case "users":
                        bot.RefreshUser();
                        Console.WriteLine("<user>");
                        foreach (long userID in bot.users.Keys)
                        {
                            ChatUser user = bot.users[userID];
                            string uName = user._user.Username != null ? user._user.Username : user._user.FirstName;

                            Console.WriteLine("\t" + uName + @" | Last Command: """ + user.LastMessage + @""" (" + user.LastMessageTime + ")");
                        }
                        Console.WriteLine(@"</user>");
                        break;
                    case "chats":
                    case "rooms":
                    case "groups":
                        bot.RefreshChats();
                        Console.WriteLine("<chats>");
                        foreach (long chatID in bot.chats.Keys)
                        {
                            Chat chat = bot.chats[chatID];
                            string chatName = chat.Type == Telegram.Bot.Types.Enums.ChatType.Private ? chat.Username : chat.Title;
                            Console.WriteLine("\t[" + chat.Type + "] " + chatName + " (" + chatID + ")");
                        }
                        Console.WriteLine(@"</chats>");
                        break;
                    case "sendtochat":
                        if (param.Count > 0)
                        {
                            Chat targetChat = null;
                            try
                            {
                                long id = long.Parse(param[0]);
                                targetChat = bot.GetChatByID(id);
                            }
                            catch
                            {
                                targetChat = bot.GetChatByName(param[0]);
                            }
                            if (targetChat == null)
                            {
                                Console.WriteLine("Chat not found");
                            }
                            else
                            {
                                Console.Write("Message: ");
                                bot.SendMessage(targetChat.Id, Console.ReadLine());
                            }
                        }

                        break;
                }
                Console.WriteLine("");
                Console.Write(">_ ");
                command = Console.ReadLine();
                Console.WriteLine("");
            }
            try
            {
                bot._bot.StopReceiving();
            }
            catch { }
        }
    }
}
