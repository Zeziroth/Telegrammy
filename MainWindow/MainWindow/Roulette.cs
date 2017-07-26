using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MainWindow
{
    class Roulette
    {
        private static Bot _bot = null;

        private long id;
        private static Dictionary<long, Roulette> games = new Dictionary<long, Roulette>();
        private static List<long> historyGames = new List<long>();
        private List<ChatUser> members = new List<ChatUser>();
        private ChatUser startUser = null;
        private bool open = true;
        private int maxUser;
        private int curMember;

        public bool isOpen()
        {
            return open;
        }
        private int HistoryCount()
        {
            int i = 0;
            foreach (long chatID in historyGames)
            {
                if (chatID == id)
                {
                    i++;
                }
            }
            return i + 1;
        }
        public static void Init(Bot bot)
        {
            if (_bot == null)
            {
                _bot = bot;
            }
        }
        public void Shoot(ChatUser user)
        {
            if (members[curMember]._user.Id == user._user.Id)
            {
                Random rnd = new Random();
                int luck = rnd.Next(0, 100);

                if (luck <= 75)
                {
                    SafePlayer();
                }
                else
                {
                    KillPlayer();
                }
            }
        }
        public static Roulette GetGame(long chatID)
        {
            try
            {
                return games[chatID];
            }
            catch
            {
                return null;
            }
        }
        public void Abort(ChatUser sender)
        {
            if (startUser._user.Id == sender._user.Id)
            {
                historyGames.Add(id);
                games.Remove(id);
                _bot.SendMessage(id, "Da Spiel wurde vom Veranstalter abgebrochen!");
            }
        }
        private void NextPlayer(bool random = false)
        {
            Thread.Sleep(500);
            Random rnd = new Random();
            int newNo = random ? rnd.Next(0, MemberCount()) : curMember + 1;

            if (newNo >= MemberCount())
            {
                newNo = 0;
            }
            curMember = newNo;
            Send(id, members[curMember]._user.FirstName + " ist jetzt an der Reihe.");
        }
        private void SafePlayer()
        {
            Send(id, "KLICK! " + members[curMember]._user.FirstName + " hat Glück gehabt");
            NextPlayer();
        }
        private static void Send(long id, string msg)
        {
            Roulette table = GetGame(id);
            _bot.SendMessageHTML(id, "<code>Roulette Runde " + table.HistoryCount() + " (" + table.members.Count() + "/" + table.maxUser + ")</code>" + Environment.NewLine + "<i>" + msg + "</i>");
        }
        private void KillPlayer()
        {
            string msg = "🔫 PENG! " + members[curMember]._user.FirstName + " hat sich erschossen. Viel Glück beim nächsten mal!";
            members.RemoveAt(curMember);

            if (MemberCount() > 1)
            {
                Send(id, msg);
                NextPlayer();
                return;
            }
            else
            {
                msg += Environment.NewLine + Environment.NewLine + "🎉 GEWINNER! " + Environment.NewLine + members[0]._user.FirstName + " ist der Sieger dieser brutalen Runde, Glückwunsch!";
                Send(id, msg);
                historyGames.Add(id);
                games.Remove(id);
            }

            
        }
        private Roulette(long chatID, ChatUser starter, int max)
        {
            id = chatID;

            maxUser = max;
            curMember = 0;
            startUser = starter;
            members.Add(starter);
        }
        public void AddMember(ChatUser user)
        {
            if (open && !members.Contains(user))
            {
                members.Add(user);
                string msg = "🙋 " + user._user.FirstName + " spielt nun mit!(" + MemberCount() + " / " + maxUser + ")";
                if (MemberCount() == maxUser)
                {
                    open = false;
                    msg += Environment.NewLine + "🏁 Das spiel beginnt..." + Environment.NewLine + "Wenn du an der Reihe bist musst du /shoot eingeben";
                    _bot.SendMessageHTML(id, msg);
                    NextPlayer(true);
                    return;
                }
                _bot.SendMessageHTML(id, msg);
            }
        }
        public int MemberCount()
        {
            return members.Count();
        }
        public static void StartGame(long chatID, ChatUser starter, int max)
        {
            if (max > 1 && GetGame(chatID) == null)
            {
                games.Add(chatID, new Roulette(chatID, starter, max));
                Send(chatID, starter._user.FirstName + " hat eine Runde Roulette gestartet!" + Environment.NewLine + "Wer mitspielen möchte muss /roulette eingeben.");
            }
        }
    }
}
