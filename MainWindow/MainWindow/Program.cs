using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
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

            while (command != "end")
            {
                switch (command)
                {
                    case "me":
                        Console.WriteLine(bot.Data.FirstName + " " + bot.Data.LastName + " (" + bot.Data.Username + ")");
                        break;
                }
                command = Console.ReadLine();
            }
            try
            {
                bot._bot.StopReceiving();
            }
            catch { }
        }
    }
}
