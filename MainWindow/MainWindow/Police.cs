using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
namespace MainWindow
{
    class Police
    {
        private string _keyword { get; set; }
        private List<HtmlDocument> _sourcePages { get; set; }
        private List<string> _articles { get; set; }
        public Police(string keyword)
        {
            _keyword = keyword;
        }
        private void Load(int articleNum)
        {
            _sourcePages = new List<HtmlDocument>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HTTPRequester.SimpleRequest("https://www.presseportal.de/blaulicht/r/" + _keyword + "/" + (articleNum - (articleNum % 27))));
            _sourcePages.Add(doc);
        }
        public string PrintArticle(int i = 0)
        {
            i -= 1;
            _articles = new List<string>();
            int articleNum = 0;
            i = i > 0 ? i : 0;
            Load(i);
            articleNum = (i - (i % 27));

            foreach (HtmlDocument doc in _sourcePages)
            {
                List<HtmlNode> bigContainer = GetDivsByClass(doc.DocumentNode, "grid-container");
                HtmlNode mainContainer = bigContainer[0];

                IEnumerable<HtmlNode> articles = GetElementsByTagName(doc.DocumentNode, "article");

                Console.WriteLine("Found: " + articles.Count() + " articles");
                foreach (HtmlNode article in articles)
                {
                    if (articleNum == i)
                    {
                        string timestamp = GetElementsByClass(article, "span", "news-date sans")[0].InnerText;
                        HtmlNode headlineNode = GetElementsByClass(article, "h2", "news-headline news-headline-clamp")[0];
                        string headline = GetElementsByTagName(headlineNode, "span").First().InnerText;
                        string desc = GetDivByClass(article, "news-bodycopy").InnerText;
                        string hrefLink = article.Attributes["data-url"].Value;
                        return "<code>[" + timestamp.Split(' ')[0] + "] " + headline + "</code>" + Environment.NewLine + Environment.NewLine + desc + Environment.NewLine + "<a href=\"https://www.presseportal.de/" + hrefLink + "\">Weiterlesen..</a>";
                    }
                    articleNum++;
                }
            }
            Console.WriteLine("Nothing found");
            return "<code>Presseinformationen konnte nicht geladen werden!</code>";
        }
        public static IEnumerable<HtmlNode> GetElementsByTagName(HtmlNode parent, string name)
        {
            return parent.Descendants(name);
        }
        private static HtmlNode GetDivByClass(HtmlNode docNode, string className)
        {
            IEnumerable<HtmlNode> newNodes = GetElementsByTagName(docNode, "div");

            foreach (HtmlNode node in newNodes)
            {
                if (node.Attributes.Contains("class") && node.Attributes["class"].Value == className)
                {
                    return node;
                }
            }

            return null;
        }
        private static List<HtmlNode> GetElementsByClass(HtmlNode docNode, string elmName, string className)
        {
            IEnumerable<HtmlNode> newNodes = GetElementsByTagName(docNode, elmName);

            List<HtmlNode> outputNodes = new List<HtmlNode>();
            foreach (HtmlNode node in newNodes)
            {
                if (node.Attributes.Contains("class") && node.Attributes["class"].Value == className)
                {
                    outputNodes.Add(node);
                }
            }

            return outputNodes;
        }
        private static List<HtmlNode> GetDivsByClass(HtmlNode docNode, string className)
        {
            IEnumerable<HtmlNode> newNodes = GetElementsByTagName(docNode, "div");
            List<HtmlNode> outputNodes = new List<HtmlNode>();
            foreach (HtmlNode node in newNodes)
            {
                if (node.Attributes.Contains("class") && node.Attributes["class"].Value == className)
                {
                    outputNodes.Add(node);
                }
            }

            return outputNodes;
        }
    }
}
