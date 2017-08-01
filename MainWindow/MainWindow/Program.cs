using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot.Types;
namespace MainWindow
{
    class Program
    {
        private static CommandController cController = null;
        private static string command = "";
        private static Bot bot = null;
        private static string[] endCommands = new string[] { "end", "close", "exit" };
        private static Dictionary<List<string>, Dictionary<string, Action>> commands = new Dictionary<List<string>, Dictionary<string, Action>>() { };

        private static List<string> param = new List<string>();
        static void Main(string[] args)
        {
            Console.WriteLine("Initialising...");
            InitCommands();
            bot = new Bot(Settings.API_KEY);

            while (bot.Data == null) { }

            Console.WriteLine("Connected as: " + bot.Data.Username);
            DBController.Init();
            bot.Init();
            Settings.ignoreInput = false;
            while (!endCommands.Contains(command))
            {

                string cmd = command.Contains(' ') ? command.Split(' ')[0] : command;
                param = command.Split(' ').ToList();
                param.RemoveAt(0);
                cController.HandleCommand(cmd);
                Thread.Sleep(25);
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
        private static void SendToChat()
        {
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
        }
        private static void PrintChats()
        {
            bot.RefreshChats();
            Console.WriteLine("<chats>");
            foreach (long chatID in bot.chats.Keys)
            {
                Chat chat = bot.chats[chatID];
                string chatName = chat.Type == Telegram.Bot.Types.Enums.ChatType.Private ? chat.Username : chat.Title;
                Console.WriteLine("\t[" + chat.Type + "] " + chatName + " (" + chatID + ")");
            }
            Console.WriteLine(@"</chats>");
        }
        private static void PrintUser()
        {
            bot.RefreshUser();
            Console.WriteLine("<user>");
            foreach (long userID in bot.users.Keys)
            {
                ChatUser user = bot.users[userID];
                string uName = user.Username();

                Console.WriteLine("\t" + uName + @" | Last Command: """ + user.LastMessage + @""" (" + user.LastMessageTime + ")");
            }
            Console.WriteLine(@"</user>");
        }
        private static void PrintBotInformations()
        {
            Console.WriteLine(bot.Data.FirstName + " " + bot.Data.LastName + " (" + bot.Data.Username + ")");
        }
        private static bool ValidCommand(string cmd)
        {
            string term = cmd.ToLower();
            foreach (List<string> commandTree in commands.Keys)
            {
                if (commandTree.Contains(term))
                {
                    return true;
                }
            }
            return false;
        }
        private static void InitCommands()
        {
            commands.Add(new List<string>() { "clear" }, new Dictionary<string, Action>() { { "Löscht das aktuelle Konsolenfenster.", Console.Clear } });
            commands.Add(new List<string>() { "me" }, new Dictionary<string, Action>() { { "Zeigt Informationen über den aktuellen Bot an.", PrintBotInformations } });
            commands.Add(new List<string>() { "member", "members", "user", "users" }, new Dictionary<string, Action>() { { "Zeigt Informationen über Benutzer an, welche den Bot genutzt haben.", PrintUser } });
            commands.Add(new List<string>() { "chats", "chat", "rooms", "groups", "group" }, new Dictionary<string, Action>() { { "Löscht das aktuelle Konsolenfenster.", PrintChats } });
            commands.Add(new List<string>() { "sendtochat" }, new Dictionary<string, Action>() { { "Sendet eine Nachricht an einen bestimmten Chat,", SendToChat } });
            cController = new CommandController(ref commands);
        }
    }
}
