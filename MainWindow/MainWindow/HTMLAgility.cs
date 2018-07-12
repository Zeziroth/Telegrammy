using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace MainWindow
{
    public static class HTMLAgility
    {
        public static List<HtmlNode> GetElementsByTagName(HtmlNode parent, string name)
        {
            List<HtmlNode> nodes = new List<HtmlNode>();
            IEnumerable<HtmlNode> newNodes = parent.Descendants(name);
            foreach (HtmlNode node in newNodes)
            {
                nodes.Add(node);
            }
            return nodes;
        }
        public static HtmlNode GetDivByClass(HtmlNode docNode, string className)
        {
            List<HtmlNode> newNodes = GetElementsByTagName(docNode, "div");

            foreach (HtmlNode node in newNodes)
            {
                if (node.Attributes.Contains("class") && node.Attributes["class"].Value == className)
                {
                    return node;
                }
            }

            return null;
        }
        public static List<HtmlNode> GetElementsByClass(HtmlNode docNode, string elmName, string className)
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
        public static List<HtmlNode> GetDivsByClass(HtmlNode docNode, string className, bool noSpace = false)
        {
            IEnumerable<HtmlNode> newNodes = GetElementsByTagName(docNode, "div");
            List<HtmlNode> outputNodes = new List<HtmlNode>();
            foreach (HtmlNode node in newNodes)
            {
                
                if (node.Attributes.Contains("class"))
                {
                    string checkString = noSpace ? node.Attributes["class"].Value.Replace(" ", "") : node.Attributes["class"].Value;
                    if (noSpace && checkString.StartsWith("big"))
                    {
                        //System.Windows.Forms.MessageBox.Show(checkString);
                    }
                    if (checkString == className)
                    {
                        outputNodes.Add(node);
                    }
                }
            }

            return outputNodes;
        }
    }
}
