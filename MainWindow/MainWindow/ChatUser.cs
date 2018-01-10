using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MainWindow
{
    public class ChatUser
    {
        public static Dictionary<long, ChatUser> AllUser { get; private set; }

        public Telegram.Bot.Types.User _user { get; set; }
        public Stopwatch Timer { get; set; }
        public DateTime LastMessageTime { get; set; }
        public string LastMessage { get;  set; }
        public ChatUser() { }

        private ChatUser(Telegram.Bot.Types.User user)
        {
            Timer = new Stopwatch();
            _user = user;
            LastMessage = "";
            
        }
        public string Username()
        {
            return _user.Username != null ? _user.Username : _user.FirstName;
        }

        public bool isSpamming()
        {
            DateTime now = DateTime.Now;
            TimeSpan diff = now.Subtract(LastMessageTime);
            if (diff.Seconds >= Settings.SPAM_SECONDS)
            {
                return false;
            }
            return true;
        }
        public static ChatUser GetUser(Telegram.Bot.Types.User user)
        {
            if (AllUser == null)
            {
                AllUser = new Dictionary<long, ChatUser>();
            }

            if (AllUser.ContainsKey(user.Id))
            {
                return AllUser[user.Id];
            }

            AllUser.Add(user.Id, new ChatUser(user));

            return AllUser[user.Id];
        }
        public bool OnMessageReceived(string msg)
        {
            if (!isSpamming())
            {
                LastMessageTime = DateTime.Now;
                LastMessage = msg;
                return true;
            }
            return false;
        }
    }
}
