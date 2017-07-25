using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindow
{
    class Roulette
    {
        private static Bot _bot = null;

        private long id;
        private static Dictionary<long, Roulette> games = new Dictionary<long, Roulette>();
        private List<ChatUser> members = new List<ChatUser>();
        private bool open = true;
        private int maxUser;
        private int curMember;

        public bool isOpen()
        {
            return open;
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
            Console.WriteLine(user._user.FirstName + "is trying to shoot");

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

        private void NextPlayer(bool random = false)
        {
            Random rnd = new Random();
            int newNo = random ? rnd.Next(0, MemberCount()) : curMember + 1;

            if (newNo >= MemberCount())
            {
                newNo = 0;
            }
            curMember = newNo;
            _bot.SendMessage(id, members[curMember]._user.FirstName + " ist jetzt an der Reihe.");
        }

        private void SafePlayer()
        {
            _bot.SendMessage(id, "KLICK! " + members[curMember]._user.FirstName + " hat sich eingeschissen, aber ist nicht gestorben.");
            NextPlayer();
        }

        private void KillPlayer()
        {
            _bot.SendMessage(id, members[curMember]._user.FirstName + " hat sich erschossen. Viel Glück beim nächsten mal!");
            members.RemoveAt(curMember);
            if (MemberCount() > 1)
            {
                NextPlayer();
            }
            else
            {
                _bot.SendMessage(id, members[0]._user.FirstName + " ist der Sieger dieser brutalen Runde, Glückwunsch!");
                games.Remove(id);
            }
        }

        private Roulette(long chatID, ChatUser starter, int max)
        {
            id = chatID;
            maxUser = max;
            curMember = 0;
            members.Add(starter);
        }

        public void AddMember(ChatUser user)
        {
            if (open && !members.Contains(user))
            {
                members.Add(user);
                _bot.SendMessage(id, user._user.FirstName + " spielt nun mit!  (" + MemberCount() + " / " + maxUser + ")");

                if (MemberCount() == maxUser)
                {
                    open = false;
                    _bot.SendMessage(id, "Das spiel beginnt...");
                    NextPlayer(true);
                }
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
                _bot.SendMessage(chatID, starter._user.FirstName + " hat eine Runde Roulette gestartet! Wer mitspielen möchte muss /roulette eingeben");
            }
        }
    }
}
