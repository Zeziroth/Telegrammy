using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindow
{
    class Qwertee
    {
        private static DateTime newTeeInterval = new DateTime();
        private static List<Tee> currentTees = new List<Tee>();

        public static List<Tee> LatestTees()
        {
            List<Tee> tees = new List<Tee>();

            HtmlDocument doc = new HtmlDocument();
            DateTime now = DateTime.Now;
            doc.LoadHtml(HTTPRequester.SimpleRequest("https://www.qwertee.com/"));
            string mainNode = doc.DocumentNode.InnerHtml;
            HtmlNode countdown = HTMLAgility.GetDivByClass(doc.DocumentNode, "index-countdown");

            string secondsLeft = countdown.GetAttributeValue("data-time", "");
            DateTime finalTime = now.AddSeconds(int.Parse(secondsLeft));

            if (newTeeInterval == DateTime.MinValue)
            {
                newTeeInterval = finalTime.AddSeconds(10);
            }
            else if(finalTime > newTeeInterval)
            {
                newTeeInterval = finalTime.AddSeconds(10);
            }
            else
            {
                return currentTees;
            }
            currentTees.Clear();

            List<HtmlNode> teeNodes = HTMLAgility.GetDivsByClass(doc.DocumentNode, "big-slidetee\ntee-last-chance", true);
            
            foreach (HtmlNode teeNode in teeNodes)
            {
                
                HtmlNode masterTee = HTMLAgility.GetDivByClass(teeNode, "index-tee ");
                string title = masterTee.GetAttributeValue("data-name", "");
                string price = masterTee.GetAttributeValue("data-tee-price-eur", "");
                string picture = HTMLAgility.GetElementsByTagName(teeNode, "source")[0].GetAttributeValue("srcset", "");

                if (currentTees.Count < 3 && currentTees.Where(t => t.title == title).Count() == 0)
                {
                    currentTees.Add(new Tee(title, price, picture));
                }
            }

            return currentTees;
        }
    }
}
