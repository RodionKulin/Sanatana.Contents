using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sanatana.Contents;
using Sanatana.Contents.Html.HtmlNodes;

namespace Sanatana.Contents.Html
{
    public class HtmlTagRemover
    {
        //const
        private static string[] NEW_LINE_TAGS = new string[] { "p", "div", "ul", "li", "table", "td", "blockquote" };

        private static string[] SKIP_TAGS = new string[] { "iframe" };



        //removing html tags
        public static string StripHtml(string html)
        {
            HtmlRootElement root = HtmlParser.Parse(html, true);
            return StripHtml(root);
        }

        public static string StripHtml(HtmlElement element)
        {
            StringBuilder builder = new StringBuilder();
            StripHtml(builder, element);

            string result = builder.ToString();
            return Regex.Replace(result, @"&nbsp;|\s+", " ");
        }

        private static void StripHtml(StringBuilder builder, HtmlElement element)
        {
            foreach (HtmlNode node in element.ChildNodes)
            {
                if (node is HtmlText)
                {
                    builder.Append(node.ToString());
                }
                else if (node is HtmlElement)
                {
                    HtmlElement child = node as HtmlElement;

                    if(SKIP_TAGS.Contains(child.Name))
                    {
                        AddWhitespace(builder);
                    }
                    else if (NEW_LINE_TAGS.Contains(child.Name))
                    {
                        AddWhitespace(builder);
                        StripHtml(builder, child);
                    }                    
                }
            }
        }

        private static void AddWhitespace(StringBuilder builder)
        {
            if (builder.Length > 0
                && !char.IsWhiteSpace(builder[builder.Length - 1]))
            {
                builder.Append(" ");
            }
        }



        //remove empty tags with their content(whitespaces and new lines)
        public static string RemoveEmptyTags(string input)
        {
            HtmlRootElement root = HtmlParser.Parse(input);
            bool hasContent = HasContent(root);

            if (hasContent)
            {
                return root.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static void RemoveEmptyTags(HtmlElement document)
        {
            bool hasContent = HasContent(document);
        }

        private static bool HasContent(HtmlElement element)
        {
            if (element.Name == "img" || element.Name == "iframe" || element.Name == "frame")
                return true;

            // пустой element без контейнеров и текста
            if (element.ChildNodes.Count == 0)
                return false;

            // проверить вложенные элементы и удалить пустые
            for (int i = element.ChildNodes.Count - 1; i >= 0; i--)
            {
                if (element.ChildNodes[i] is HtmlElement)
                {
                    HtmlElement child = element.ChildNodes[i] as HtmlElement;
                    bool hasContent = HasContent(child);
                    if (!hasContent)
                    {
                        element.ChildNodes.RemoveAt(i);
                    }
                }
                else if (element.ChildNodes[i] is HtmlComment
                    || element.ChildNodes[i] is HtmlInstruction)
                {
                    element.ChildNodes.RemoveAt(i);
                }
            }

            // если нету вложенныех элементов (элементов контейнеров и элементов текстов)
            if (element.ChildNodes.Count == 0)
                return false;

            // проверить тексты (если есть хоть один контейнер, то ищём контент в нём)
            if (!element.ChildNodes.Any(p => p is HtmlElement))
            {
                bool hasText = HasText(element);
                if (!hasText)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasText(HtmlElement element)
        {
            StringBuilder content = new StringBuilder();
            for (int j = 0; j < element.ChildNodes.Count; j++)
            {
                if (element.ChildNodes[j] is HtmlText)
                {
                    string nodeText = element.ChildNodes[j].ToString().ToLower();
                    content.Append(nodeText);
                }
            }

            string whiteSpacesExpression = @"&nbsp;|\s+|\t|\n|\r|" + Environment.NewLine;
            string pureText = Regex.Replace(content.ToString(), whiteSpacesExpression, string.Empty);
            return pureText != string.Empty;
        }
    }
}
