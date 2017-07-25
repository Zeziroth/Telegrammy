using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;

namespace MainWindow
{
    class Bot
    {
        public string _key { get; private set; }
        public TelegramBotClient _bot { get; private set; }
        public Telegram.Bot.Types.User Data { get; private set; }
        public List<long> chatroomIDs { get; private set; }

        public Bot(string key)
        {
            Start(key);
            Roulette.Init(this);
        }

        public bool RegisterChat(long id)
        {
            if (!chatroomIDs.Contains(id))
            {
                chatroomIDs.Add(id);
                Console.WriteLine("Room registered! (" + id + ")");
                return true;
            }
            Console.WriteLine("Room already registered! (" + id + ")");
            return false;
        }
        private async void Start(string key)
        {
            try
            {
                _key = key;
                var bot = new TelegramBotClient(Settings.API_KEY);
                _bot = bot;
                Data = await bot.GetMeAsync();
                Console.WriteLine("Initialise successfully....");
                Console.WriteLine("Setting up events...");
                _bot.OnCallbackQuery += OnCallbackQueryReceived;
                _bot.OnMessage += OnMessageReceived;
                _bot.OnMessageEdited += OnMessageReceived;
                _bot.OnInlineQuery += OnInlineQueryReceived;
                _bot.OnInlineResultChosen += OnChosenInlineResultReceived;
                _bot.OnReceiveError += OnReceiveError;
                _bot.StartReceiving();
                Console.WriteLine("Events up...");
                chatroomIDs = new List<long>();
            }
            catch
            {
                Console.WriteLine("Error while initialising...");
            }
        }
        private static bool isAdmin(int id)
        {
            foreach (int admin in Settings.ADMINS)
            {
                if (admin == id)
                {
                    return true;
                }
            }
            return false;
        }
        private void OnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Error");
        }
        private void OnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }
        private async void OnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            await _bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, null, isPersonal: true, cacheTime: 0);
        }
        private async void OnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;


            if (message == null || message.Type != MessageType.TextMessage) return;

            Telegram.Bot.Types.User from = message.From;
            Telegram.Bot.Types.Chat chat = message.Chat;

            ChatUser user = ChatUser.GetUser(from);

            if (message.Text.StartsWith("/"))
            {
                if (user.OnMessageReceived())
                {
                    string parseCommand = message.Text.Contains(' ') ? message.Text.Split(' ')[0] : message.Text;
                    switch (parseCommand.Remove(0, 1).ToLower())
                    {
                        case "register":
                            if (isAdmin(from.Id))
                            {
                                if (RegisterChat(chat.Id))
                                {
                                    SendMessage(chat.Id, "Chat with ID " + chat.Id + " successfully registered...");
                                }
                            }
                            break;
                        case "kawaii":
                            Random rnd = new Random();
                            SendMessage(chat.Id, from.FirstName + " ist zu " + rnd.Next(1, 100) + "% Kawaii");
                            break;
                        case "roulette":
                            if (Roulette.GetGame(chat.Id) == null)
                            {
                                if (message.Text.Contains(' '))
                                {
                                    int maxMember = int.Parse(message.Text.Split(' ')[1]);
                                    Roulette.StartGame(chat.Id, user, maxMember);
                                }

                            }
                            else
                            {
                                Roulette.GetGame(chat.Id).AddMember(user);
                            }
                            break;
                        case "shoot":
                            if (Roulette.GetGame(chat.Id) != null)
                            {
                                Roulette gameTable = Roulette.GetGame(chat.Id);
                                if (!gameTable.isOpen())
                                {
                                    gameTable.Shoot(user);
                                }
                            }
                            break;
                    }
                }
            }
        }
        internal async void SendMessage(long chatID, string msg)
        {
            await _bot.SendTextMessageAsync(chatID, msg);
        }
        private async void OnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
