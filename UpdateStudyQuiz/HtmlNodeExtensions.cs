using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateStudyQuiz
{
    public static class HtmlNodeExtensions
    {
        public static HtmlNode StripAttributes(this HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                foreach (var child in node.ChildNodes)
                {
                    child.StripAttributes();
                }

                node.Attributes.RemoveAll();
            }

            return node;
        }
    }
}
