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
            try
            {
                i -= 1;
                _articles = new List<string>();
                int articleNum = 0;
                i = i > 0 ? i : 0;
                Load(i);
                articleNum = (i - (i % 27));

                foreach (HtmlDocument doc in _sourcePages)
                {
                    List<HtmlNode> bigContainer = HTMLAgility.GetDivsByClass(doc.DocumentNode, "grid-container");
                    HtmlNode mainContainer = bigContainer[0];

                    IEnumerable<HtmlNode> articles = HTMLAgility.GetElementsByTagName(doc.DocumentNode, "article");


                    foreach (HtmlNode article in articles)
                    {
                        if (articleNum == i)
                        {
                            string timestamp = HTMLAgility. GetElementsByClass(article, "span", "news-date sans")[0].InnerText;
                            HtmlNode headlineNode = HTMLAgility.GetElementsByClass(article, "h2", "news-headline news-headline-clamp")[0];
                            string headline = HTMLAgility.GetElementsByTagName(headlineNode, "span").First().InnerText;
                            string desc = HTMLAgility.GetDivByClass(article, "news-bodycopy").InnerText;
                            string hrefLink = article.Attributes["data-url"].Value;
                            return "<code>[" + timestamp.Split(' ')[0] + "] " + headline + "</code>" + Environment.NewLine + Environment.NewLine + desc + Environment.NewLine + "<a href=\"https://www.presseportal.de/" + hrefLink + "\">Weiterlesen..</a>";
                        }
                        articleNum++;
                    }
                }

                return "<code>Presseinformationen konnte nicht geladen werden!</code>";
            }
            catch
            {
                return "<code>Presseinformationen konnte nicht geladen werden!</code>";
            }
        }

    }
}
