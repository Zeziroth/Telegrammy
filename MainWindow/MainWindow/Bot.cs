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
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;

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
            new SteamFree(-209505282, this); //Insert your Channel-ID
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
            commands.Add(new List<string>() { "xrp", "ripple" }, new Dictionary<string, Action>() { { "Gibt den aktuellen Kurs XRP/$ aus.", GetXRPChart } });
            commands.Add(new List<string>() { "rtd", "dice", "rool", "random" }, new Dictionary<string, Action>() { { "Gibt dir eine zufällige Zahl zwischen deiner Mindestzahl und deiner Maxzahl aus.", Random } });
            commands.Add(new List<string>() { "dhl" }, new Dictionary<string, Action>() { { "DHL Paketverfolgung durch eingabe der Tracking-ID.", DHLTrack } });
            commands.Add(new List<string>() { "jing" }, new Dictionary<string, Action>() { { "Zeigt den Mittagstisch von Jing-Jai.", GetFoodJingJai } });
            commands.Add(new List<string>() { "police", "polizei", "pol" }, new Dictionary<string, Action>() { { "Zeigt aktuelle Presseinformationen der gewünschten Stadt an.", GetPoliceNews} });
            commands.Add(new List<string>() { "hermes" }, new Dictionary<string, Action>() { { "Hermes Paketverfolgung durch eingabe der Tracking-ID.", HermesTrack } });
            commands.Add(new List<string>() { "register" }, new Dictionary<string, Action>() { { "Registriert einen Chat permanent beim Bot.", RegisterChat } });
            commands.Add(new List<string>() { "kawaii" }, new Dictionary<string, Action>() { { "Lass den Bot entscheiden wie Kawaii du wirklich bist.", KawaiiMeter } });
            commands.Add(new List<string>() { "roulette" }, new Dictionary<string, Action>() { { "Eröffnet bzw. nimmt an einem neuen Roulettespiel teil.", RouletteHandler } });
            commands.Add(new List<string>() { "shoot", "shot" }, new Dictionary<string, Action>() { { "Wenn du in einem Roulettespiel bist, kannst du hiermit deinen Schuss tätigen.", ShootHandler } });
            commands.Add(new List<string>() { "abort", "cancel", "bittestophabibi" }, new Dictionary<string, Action>() { { "Stopt eine vorhandene Rouletterunde (Nur für den Spielersteller)", StopRoulette } });
            cController = new CommandController(ref commands);
        }
        
        private bool isInnerWeek(string day)
        {
            switch (day.ToLower())
            {
                case "montag":
                case "dienstag":
                case "mittwoch":
                case "donnerstag":
                case "freitag":
                    return true;
                default:
                    return false;
            }
        }
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
        private void GetXRPChart()
        {
            try
            {
                if (param.Count > 1)
                {
                    int xrpAmount = int.Parse(param[1]);
                    switch (param[0])
                    {
                        case "add":
                        case "buy":
                            DBController.ExecuteQuery("INSERT INTO xrp (userID, xrpAmount, usdTicker, timestamp) VALUES ('" + user._user.Id + "', '" + xrpAmount + "', '" + GetXRPUSD().ToString().Replace(",", ".") + "', '" + Core.DateTimeToUnixTime() + "')");
                            break;

                        case "remove":
                        case "sell":
                            List<int> delIDs = new List<int>();
                            if (DBController.EntryExist("SELECT * FROM xrp WHERE userID = '" + user._user.Id + "' LIMIT 1"))
                            {
                                SQLiteDataReader readerDel = DBController.ReturnQuery("SELECT * FROM xrp WHERE userID = '" + user._user.Id + "'");

                                foreach (DbDataRecord row in readerDel)
                                {
                                    if (xrpAmount <= 0)
                                    {
                                        break;
                                    }
                                    int curID = int.Parse(row["id"].ToString());
                                    int curXRPAmount = int.Parse(row["xrpAmount"].ToString());
                                    if (curXRPAmount <= xrpAmount)
                                    {
                                        xrpAmount -= curXRPAmount;
                                        delIDs.Add(curID);
                                    }
                                    else
                                    {
                                        int newXRPAmount = curXRPAmount - xrpAmount;
                                        xrpAmount = 0;
                                        DBController.ExecuteQuery("UPDATE xrp SET xrpAmount = '" + newXRPAmount + "' WHERE id = '" + curID + "'");
                                    }
                                }
                                foreach (int delID in delIDs)
                                {
                                    DBController.ExecuteQuery("DELETE FROM xrp WHERE id = '" + delID + "'");
                                }
                            }
                            break;
                    }
                }
                else
                {
                    decimal usd = GetXRPUSD();
                    StringBuilder strBuild = new StringBuilder().AppendLine("<code>1 XRP = " + usd + "$</code>");
                    if (DBController.EntryExist("SELECT * FROM xrp WHERE userID = '" + user._user.Id + "' LIMIT 1"))
                    {
                        strBuild.AppendLine("");
                        SQLiteDataReader reader = DBController.ReturnQuery("SELECT * FROM xrp WHERE userID = '" + user._user.Id + "'");
                        foreach (DbDataRecord row in reader)
                        {
                            decimal oldTotalUSD = decimal.Parse(row["xrpAmount"].ToString()) * decimal.Parse(row["usdTicker"].ToString());
                            decimal newTotalUSD = decimal.Parse(row["xrpAmount"].ToString()) * usd;
                            decimal difference = Math.Round(newTotalUSD - oldTotalUSD, 2);
                            strBuild.AppendLine("<code>[" + Core.UnixTimeStampToDateTime(double.Parse(row["timestamp"].ToString())).ToString("d.M.yy HH:mm") + "] " + row["xrpAmount"] + "XRP (" + row["usdTicker"].ToString() + "$) => " + difference + "$ Profit</code>");
                        }
                    }
                    SendMessageHTML(chat.Id, strBuild.ToString());
                }
            }
            catch
            {
                SendMessageHTML(chat.Id, "Versuche es bitte später erneut.");
            }
        }
        private decimal GetXRPUSD()
        {
            string jsonChartPlain = HTTPRequester.SimpleRequest("https://www.binance.com/api/v1/ticker/allPrices");
            List<BinancePair> jsonChart = JsonConvert.DeserializeObject<List<BinancePair>>(jsonChartPlain);
            BinancePair xrpeth = jsonChart.Where((s) => s.symbol == "XRPETH").First();
            BinancePair ethusdt = jsonChart.Where((s) => s.symbol == "ETHUSDT").First();
            decimal usdTicker = Math.Round((decimal.Parse(xrpeth.price.Replace(".", ",")) * decimal.Parse(ethusdt.price.Replace(".", ","))), 2);
            return usdTicker;
        }
        private void GetPoliceNews()
        {
            if (param.Count > 0)
            {
                try
                {
                    Police police = new Police(param[0].ToLower());
                    if (param.Count > 1)
                    {
                        SendMessageHTML(chat.Id, police.PrintArticle(int.Parse(param[1])));
                    }
                    else
                    {
                        SendMessageHTML(chat.Id, police.PrintArticle(0));
                    }
                    
                }
                catch
                {
                    SendMessageHTML(chat.Id, "<code>Presseinformationen konnte nicht geladen werden!</code>");
                }
            }
        }
        private void GetFoodJingJai()
        {
            if (param.Count > 0)
            {
                try
                {
                    string chosenDay = param[0].ToLower();
                    switch (chosenDay.ToLower())
                    {
                        case "gestern":
                            chosenDay = DateTime.Now.AddDays(-2).ToString("dddd").ToLower();
                            break;
                        case "vorgestern":
                            chosenDay = DateTime.Now.AddDays(-1).ToString("dddd").ToLower();
                            break;
                        case "heute":
                            chosenDay = DateTime.Now.ToString("dddd").ToLower();
                            break;

                        case "morgen":

                            chosenDay = DateTime.Now.AddDays(1).ToString("dddd").ToLower();
                            break;

                        case "übermorgen":

                            chosenDay = DateTime.Now.AddDays(2).ToString("dddd").ToLower();
                            break;
                    }
                    Console.WriteLine(chosenDay);
                    if (!isInnerWeek(chosenDay))
                    {
                        return;
                    }
                    string response = HTTPRequester.SimpleRequest("http://www.jing-jai-bremen.de/mittagstisch/");
                    string mainContent = TextHelper.StringBetweenStrings(response, @"<div id=""content_area"">", @"</p> </div>");
                    string[] lines = mainContent.Split(new string[] { "</p>" }, StringSplitOptions.None);

                    bool innerDay = false;
                    JingJai curDay = null;
                    List<JingJai> allDays = new List<JingJai>();
                    foreach (string line in lines)
                    {
                        if (!innerDay)
                        {
                            if (line.TrimStart().StartsWith(@"<p><strong><span style=""background:yellow;"">"))
                            {
                                string day = TextHelper.StringBetweenStrings(line, @"<span style=""font-size:12.0pt;"">", "</span>");

                                switch (day.ToLower())
                                {
                                    case "mo.":
                                        day = "Montag";
                                        break;
                                    case "di.":
                                        day = "Dienstag";
                                        break;
                                    case "mi.":
                                        day = "Mittwoch";
                                        break;
                                    case "do.":
                                        day = "Donnerstag";
                                        break;
                                    case "fr.":
                                        day = "Freitag";
                                        break;
                                }
                                if (day.ToLower() == chosenDay.ToLower())
                                {
                                    innerDay = true;
                                    string title = TextHelper.StringBetweenStrings(line, @"<span style=""background:aqua;"">", "</span>").Replace("`", "").Replace("´", "");
                                    curDay = new JingJai(day, title);
                                }

                            }
                        }
                        else if (line.TrimStart().StartsWith(@"<p><strong><span style=""font-family:eras bold itc,sans-serif;""><span style=""font-size:12.0pt;"">"))
                        {

                            string desc = TextHelper.StringBetweenStrings(line, @"<p><strong><span style=""font-family:eras bold itc,sans-serif;""><span style=""font-size:12.0pt;"">", "").Replace("  ", "");
                            desc = desc.Replace(Environment.NewLine, String.Empty);
                            desc = Regex.Replace(desc, @"<span style=""color:red;""><sup>[a-z]</sup>", String.Empty);
                            desc = Regex.Replace(desc, @"<sup>[a-z]", String.Empty);


                            desc = Regex.Replace(desc, @"<sup><span style=""color:red;"">[a-z]</span></sup>", String.Empty);
                            desc = Regex.Replace(desc, @"<span style=""color:red;"">[a-z]</span></sup>", String.Empty);
                            desc = Regex.Replace(desc, @"<span style=""color:red;"">[a-z]</span>", String.Empty);
                            desc = Regex.Replace(desc, @"<span style=""color:red;"">[a-z]", String.Empty);
                            desc = Regex.Replace(desc, @"<[a-z]*>(.*?)<\/[a-z]*>", String.Empty);
                            desc = Regex.Replace(desc, @"<[a-z]*>", String.Empty);
                            desc = Regex.Replace(desc, @"<\/[a-z]*>", String.Empty);
                            desc = desc.Replace("€", String.Empty);
                            if (desc.Contains("color:red;"))
                            {
                                desc = Regex.Replace(desc, @"<span style=""color:red;"">.*", String.Empty);
                                curDay.AddDesc(desc);
                                innerDay = false;
                                allDays.Add(curDay);
                            }
                            else
                            {
                                curDay.AddDesc(desc);
                            }

                        }
                    }

                    foreach (JingJai day in allDays)
                    {
                        if (day.day.ToLower() == chosenDay.ToLower())
                        {
                            SendMessageHTML(chat.Id, "<code>Am " + day.day + " gibt es bei Jing-Jai</code>" + Environment.NewLine + "<b>" + day.title.TrimStart() + "</b>" + Environment.NewLine + Environment.NewLine + day.desc.Replace(" ", "").TrimStart());
                            return;
                        }
                    }
                    SendMessageHTML(chat.Id, "<code>Jing-Jai bietet diesen " + chosenDay + " kein Essen an!</code>");
                }
                catch { }
            }
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
        private void HermesTrack()
        {
            if (param.Count > 0)
            {
                string trackingID = param[0];
                CookieContainer cookieCon = new CookieContainer();
                string responseFirst = HTTPRequester.SimpleRequest("https://www.myhermes.de/wps/portal/paket/Home/privatkunden/privatkunden", cookieCon);

                string actionURL = TextHelper.StringBetweenStrings(responseFirst, @"<form name=""mhStatusForm"" id=""mhStatusForm"" action=""", @""" onsubmit=");
                string responseFinal = HTTPRequester.SimpleRequest("https://www.myhermes.de" + actionURL + "?action=trace&shipmentID=" + trackingID + "&receiptID=", cookieCon);
                if (responseFinal.Contains("content_table table_shipmentDetails"))
                {
                    string cutFirst = TextHelper.StringBetweenStrings(responseFinal, @"<th class=""stateCol""><span>Status</span></th>", "</tbody>");
                    string cutSecond = TextHelper.StringBetweenStrings(cutFirst, @"</tr>", "</tr>");
                    string[] infos = new string[3];
                    int i = 0;
                    foreach (string line in cutSecond.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        if (line.Contains("<td>"))
                        {
                            infos[i] = line.Replace("<td>", "").Replace("</td>", "").TrimStart();
                            i++;
                        }
                    }

                    SendMessage(chat.Id, String.Join(Environment.NewLine, infos));
                    return;
                }
                SendMessage(chat.Id, "Dein Paket kann zurzeit nicht gefunden werden.");
                Console.WriteLine();
                return;
                //if (ort == "")
                //{
                //    SendMessage(chat.Id, "Dein Paket kann zurzeit nicht gefunden werden.");
                //}
                //else
                //{
                //    SendMessage(chat.Id, status + Environment.NewLine + "Ort: " + ort + " (" + timestamp + ")");
                //}

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

                string chatName = tempChat.Type == ChatType.Private ? tempChat.Username : tempChat.Title;

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
                chats = new Dictionary<long, Chat>();
                users = new Dictionary<long, ChatUser>();
                Console.WriteLine("Setting up events...");
                _bot.OnCallbackQuery += OnCallbackQueryReceived;
                _bot.OnMessage += OnMessageReceived;
                _bot.OnMessageEdited += OnMessageReceived;
                _bot.OnInlineQuery += OnInlineQueryReceived;
                _bot.OnInlineResultChosen += OnChosenInlineResultReceived;
                _bot.OnReceiveError += OnReceiveError;
                _bot.StartReceiving();
                Console.WriteLine("Events up...");
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
            try
            {
                await _bot.SendTextMessageAsync(chatID, msg, disableNotification: disableNotification);
            }
            catch { Console.WriteLine("Error sending message to: " + msg); }
        }

        private async void OnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
