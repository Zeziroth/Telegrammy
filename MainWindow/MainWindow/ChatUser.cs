using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindow
{
    class ChatUser
    {
        public static Dictionary<long, ChatUser> AllUser { get; private set; }

        public Telegram.Bot.Types.User _user { get; private set; }
        public Stopwatch Timer { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        public string LastMessage { get; private set; }

        private ChatUser(Telegram.Bot.Types.User user)
        {
            Timer = new Stopwatch();
            _user = user;
            LastMessage = "";
        }

        public bool isSpamming()
        {
            DateTime now = DateTime.Now;
            TimeSpan diff = now.Subtract(LastMessageTime);
            if (diff.Seconds > Settings.SPAM_SECONDS)
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
        public bool OnMessageReceived()
        {
            if (!isSpamming())
            {
                LastMessageTime = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}
