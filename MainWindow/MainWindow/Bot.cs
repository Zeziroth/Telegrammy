using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.Data.Common;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace MainWindow
{
    class Bot
    {
        public string _key { get; private set; }
        public TelegramBotClient _bot { get; private set; }
        public Telegram.Bot.Types.User Data { get; private set; }
        public Dictionary<long, Chat> chats { get; private set; }
        public Dictionary<long, ChatUser> users { get; private set; }
        private static Dictionary<List<string>, Dictionary<string, Action>> commands = new Dictionary<List<string>, Dictionary<string, Action>>() { };
        private static List<string> param = null;
        private static Telegram.Bot.Types.User from = null;
        private static Telegram.Bot.Types.Chat chat = null;
        private static ChatUser user = null;
        Message message = null;
        private static CommandController cController = null;

        public Bot(string key)
        {
            InitCommands();
            Start(key);
            Roulette.Init(this);
        }
        public void Init()
        {
            RefreshChats();
            RefreshUser();
        }
        private long LongRandom(long min, long max)
        {
            Random rand = new Random();
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }
        internal async void RemoveKeyboard(long chatID, string msg, bool disableNotification)
        {
            var removeKeyboard = new ReplyKeyboardRemove();
            await _bot.SendTextMessageAsync(chatID, msg, disableNotification: disableNotification, replyMarkup: removeKeyboard);
        }
        private void InitCommands()
        {
            commands.Add(new List<string>() { "rtd", "dice", "rool", "random" }, new Dictionary<string, Action>() { { "Gibt dir eine zufällige Zahl zwischen deiner Mindestzahl und deiner Maxzahl aus.", Random } });
            commands.Add(new List<string>() { "dhl" }, new Dictionary<string, Action>() { { "DHL Paketverfolgung durch eingabe der Tracking-ID.", DHLTrack } });
            commands.Add(new List<string>() { "register" }, new Dictionary<string, Action>() { { "Registriert einen Chat permanent beim Bot.", RegisterChat } });
            commands.Add(new List<string>() { "kawaii" }, new Dictionary<string, Action>() { { "Lass den Bot entscheiden wie Kawaii du wirklich bist.", KawaiiMeter } });
            commands.Add(new List<string>() { "roulette" }, new Dictionary<string, Action>() { { "Eröffnet bzw. nimmt an einem neuen Roulettespiel teil.", RouletteHandler } });
            commands.Add(new List<string>() { "shoot", "shot" }, new Dictionary<string, Action>() { { "Wenn du in einem Roulettespiel bist, kannst du hiermit deinen Schuss tätigen.", ShootHandler } });
            commands.Add(new List<string>() { "abort", "cancel", "bittestophabibi" }, new Dictionary<string, Action>() { { "Stopt eine vorhandene Rouletterunde (Nur für den Spielersteller)", StopRoulette } });
            cController = new CommandController(ref commands);
        }
        private void Random()
        {
            if (param.Count > 1)
            {
                try
                {
                    long min = long.Parse(param[0]);
                    long max = long.Parse(param[1]);

                    if (max > min)
                    {
                        long result = Core.LongRandom(min, max + 1);
                        SendMessageHTML(chat.Id, "<i>" + user.Username() + " hat eine " + result + " gewürfelt.</i>");
                    }
                }
                catch { }
            }
        }
        private void DHLTrack()
        {
            if (param.Count > 0)
            {
                string trackingID = param[0];
                string response = HTTPRequester.SimpleRequest("http://nolp.dhl.de/nextt-online-public/set_identcodes.do?lang=de&idc=" + trackingID);
                string ort = TextHelper.StringBetweenStrings(response, @"<td data-label=""Ort"">", "</td>");
                string timestamp = TextHelper.StringBetweenStrings(response, @"<td data-label=""Datum/Uhrzeit"">", "</td>");
                string status = TextHelper.StringBetweenStrings(response, @"<td data-label=""Status"">", "</td>");

                if (ort == "")
                {
                    SendMessage(chat.Id, "Dein Paket kann zurzeit nicht gefunden werden.");
                }
                else
                {
                    SendMessage(chat.Id, status + Environment.NewLine + "Ort: " + ort + " (" + timestamp + ")");
                }

            }
        }
        private void RegisterChat()
        {
            if (isAdmin(from.Id))
            {
                if (RegisterChat(chat))
                {
                    SendMessage(chat.Id, "Chat with ID " + chat.Id + " successfully registered...");
                }
            }
        }
        private void KawaiiMeter()
        {
            Random rnd = new Random();
            SendMessage(chat.Id, user.Username() + " ist zu " + rnd.Next(1, 100) + "% Kawaii");
        }
        private void RouletteHandler()
        {
            if (Roulette.GetGame(chat.Id) == null)
            {
                if (message.Text.Contains(' '))
                {
                    try
                    {
                        int maxMember = int.Parse(message.Text.Split(' ')[1]);
                        if (maxMember <= 5000)
                        {
                            Roulette.StartGame(chat.Id, user, maxMember);
                        }
                    }
                    catch { }
                }

            }
            else
            {
                Roulette.GetGame(chat.Id).AddMember(user);
            }
        }
        private void ShootHandler()
        {
            if (Roulette.GetGame(chat.Id) != null)
            {
                Roulette gameTable = Roulette.GetGame(chat.Id);
                if (!gameTable.isOpen())
                {
                    gameTable.Shoot(user);
                }
            }
        }
        private void StopRoulette()
        {
            if (Roulette.GetGame(chat.Id) != null)
            {
                Roulette gameTable = Roulette.GetGame(chat.Id);
                gameTable.Abort(user);
            }
        }
        private async void OnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {

            if (Settings.ignoreInput)
            {
                return;
            }
            message = messageEventArgs.Message;


            if (message == null || message.Type != MessageType.TextMessage) return;

            from = message.From;
            chat = message.Chat;


            if (message.Text.StartsWith("/"))
            {
                user = ChatUser.GetUser(from);
                if (user.OnMessageReceived(message.Text))
                {
                    RegisterUser(user);
                    string parseCommand = "";

                    if (message.Text.Contains("@"))
                    {
                        string toUser = message.Text.Split('@')[1].Contains(" ") ? message.Text.Split('@')[1].Split(' ')[0].ToLower() : message.Text.Split('@')[1].ToLower();
                        if (toUser == Data.Username.ToLower())
                        {
                            parseCommand = message.Text.Split('@')[0];
                        }
                        else
                        {
                            parseCommand = message.Text.Contains(' ') ? message.Text.Split(' ')[0] : message.Text;
                        }

                    }
                    else
                    {
                        parseCommand = message.Text.Contains(" ") ? message.Text.Split(' ')[0] : message.Text;
                    }

                    param = message.Text.ToString().Split(' ').ToList();
                    param.RemoveAt(0);
                    cController.HandleCommand(parseCommand.Remove(0, 1));
                }
            }
        }


        internal void RefreshChats()
        {
            if (chats != null)
            {
                chats.Clear();
            }
            SQLiteDataReader chatReader = DBController.ReturnQuery("SELECT * FROM chat");
            foreach (DbDataRecord chatRow in chatReader)
            {
                Chat curChat = JsonConvert.DeserializeObject<Chat>(chatRow["chatDATA"].ToString());
                RegisterChat(curChat, true);
            }
        }
        internal void RefreshUser()
        {
            if (users != null)
            {
                users.Clear();
            }
            SQLiteDataReader userReader = DBController.ReturnQuery("SELECT * FROM user");
            foreach (DbDataRecord userRow in userReader)
            {
                ChatUser curUser = JsonConvert.DeserializeObject<ChatUser>(userRow["userDATA"].ToString());
                RegisterUser(curUser, true);
            }
        }
        public Chat GetChatByID(long id)
        {
            foreach (long chatID in chats.Keys)
            {
                if (chatID == id)
                {
                    return chats[chatID];
                }
            }
            return null;
        }
        public Chat GetChatByName(string s)
        {
            foreach (long chatID in chats.Keys)
            {
                Chat tempChat = chats[chatID];

                string chatName = tempChat.Type == Telegram.Bot.Types.Enums.ChatType.Private ? tempChat.Username : tempChat.Title;

                if (chatName.ToLower() == s.ToLower())
                {
                    return tempChat;
                }
            }
            return null;
        }
        public bool RegisterChat(Chat chat, bool dbLoad = false)
        {
            if (!chats.ContainsKey(chat.Id))
            {
                chats.Add(chat.Id, chat);
                if (!dbLoad)
                {
                    DBController.AddChat(chat);
                }
                return true;
            }
            return false;
        }
        public bool RegisterUser(ChatUser user, bool dbLoad = false)
        {
            if (!users.ContainsKey(user._user.Id))
            {
                users.Add(user._user.Id, user);
                if (!dbLoad)
                {
                    DBController.AddUser(user);
                }
                return true;
            }
            if (!dbLoad)
            {
                DBController.AddUser(user);
            }
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
                chats = new Dictionary<long, Chat>();
                users = new Dictionary<long, ChatUser>();
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
            try
            {
                string url = "http://pr0gramm.com/api/items/get?flags=11&tags=" + inlineQueryEventArgs.InlineQuery.Query;
                string pr0List = HTTPRequester.SimpleRequest(url);

                Pr0List list = JsonConvert.DeserializeObject<Pr0List>(pr0List);
                List<InlineQueryResult> results = new List<InlineQueryResult>();
                int i = 1;
                foreach (Pr0Element itm in list.items)
                {
                    if (i < 20)
                    {
                        //Console.WriteLine(itm.GetUrl());
                        InlineQueryResult res = new InlineQueryResultMpeg4Gif
                        {
                            Id = itm.id.ToString(),
                            ThumbUrl = itm.GetUrl(),
                            Url = itm.GetUrl()
                        };
                        results.Add(res);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                await _bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results.ToArray(), isPersonal: true, cacheTime: 0);
            }
            catch
            {
                //Console.WriteLine(ex.Message);
                //await _bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, null, isPersonal: true, cacheTime: 0);
            }

        }

        internal async void SendMessageHTML(long chatID, string msg, bool disableNotification = true)
        {
            await _bot.SendTextMessageAsync(chatID, msg, ParseMode.Html, disableNotification: disableNotification);
        }

        internal async void SendMessage(long chatID, string msg, bool disableNotification = true)
        {
            await _bot.SendTextMessageAsync(chatID, msg, disableNotification: disableNotification);
        }

        private async void OnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
