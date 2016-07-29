using System;
using System.Text;


namespace ContentManagementBackend
{
    public static class HtmlParser
    {

        //методы
        public static string Tidy(string html)
        {
            HtmlElement doc = Parse(html);
            return doc.ToString();
        }

        public static HtmlRootElement Parse(string html, bool removeInstructions = false)
        {
            html = RemoveSpaces(html);

            HtmlRootElement root = new HtmlRootElement("html", false);
            AddNodes(root, html, removeInstructions);

            return root;
        }

        private static void AddNodes(HtmlElement parent, string html, bool removeInstructions)
        {
            int len = html.Length;
            int i = -1;
            string text = "";

            while (i < len - 1)
            {
                i++;

                if (i >= html.Length)
                    break;

                string s = html.Substring(i, 1);

                if (s == "<")
                {
                    int start = i + 1;
                    int stop = html.IndexOf(">", i, StringComparison.Ordinal);
                    if (stop < i)
                        continue;
                    if (text.Trim() != "")
                        parent.Add(new HtmlText(text));
                    text = "";

                    string fragment = html.Substring(start, stop - start);
                    
                    //html comment
                    if (fragment.StartsWith("!--") && fragment.EndsWith("--"))
                    {
                        parent.Add(new HtmlComment(fragment.Substring(3).Substring(0, fragment.Length - 5)));
                        i = stop;
                    }
                    //html instruction
                    else if (fragment.StartsWith("!"))
                    {
                        if (!removeInstructions)
                            parent.Add(new HtmlInstruction(fragment.Substring(1).Substring(0, fragment.Length - 1)));
                        i = stop;
                    }
                    else if (fragment.StartsWith("/"))
                    {
                        //remove invalid closing tags
                        i = stop;
                    }
                    else
                    {
                        HtmlElement node = CreateNode(html, start, ref stop);
                        parent.Add(node);
                        i = stop + (node.IsClosed ? 0 : (node.Name.Length + 2));
                    }
                    continue;
                }

                text += s;
            }

            if (text.Trim() != string.Empty)
                parent.Add(new HtmlText(text));
        }

        private static HtmlElement CreateNode(string html, int start, ref int stop)
        {
            string fragment = html.Substring(start, stop - start);
            HtmlElement node = CreateNode(fragment);
            if (node.IsClosed)
                return node;

            int ix = FindClosingTagIndex(node.Name, html, stop + 1);
            if (ix == -1)
            {
                ix = html.Length;
            }

            string contents = html.Substring(stop + 1, ix - stop - 1);

            //Do not parse script contents
            if (node.Name == "script" || node.Name == "title" || node.Name == "style")
                node.ChildNodes.Add(new HtmlText(contents));
            else
                AddNodes(node, contents, true);

            stop = ix;

            return node;
        }

        private static int FindClosingTagIndex(string tagName, string html, int startIndex)
        {
            var tag1 = "<" + tagName;
            var tag2 = "</" + tagName + ">";
            var ix = startIndex;
            while (ix > -1 && ix < html.Length)
            {

                int ix1 = html.IndexOf(tag1, ix, StringComparison.InvariantCultureIgnoreCase);
                int ix2 = html.IndexOf(tag2, ix, StringComparison.InvariantCultureIgnoreCase);

                if (ix1 == -1 && ix2 == -1)
                    break;

                if (ix2 < ix1 || (ix1 == -1 && ix2 > -1))
                    return ix2;

                //make sure we are not within script tags
                int ix3 = html.IndexOf("<script ", ix, StringComparison.InvariantCultureIgnoreCase);
                if (ix3 > -1 && ix3 < ix2)
                {
                    ix3 = html.IndexOf("</script>", ix3 + 7, StringComparison.InvariantCultureIgnoreCase);
                    if (ix3 > ix2)
                    {
                        ix = ix3 + 8;
                        continue;
                    }
                }

                ix = ix1 + tag1.Length;
            }
            return -1;
        }

        private static HtmlElement CreateNode(string htmlPart)
        {
            string html = htmlPart.Trim();
            bool isClosed = html.EndsWith("/");

            int spaceIndex = html.IndexOf(" ", StringComparison.Ordinal);
            if (spaceIndex < 0)
            {
                if(isClosed)
                    html = html.Substring(0, html.Length - 1);

                return new HtmlElement(html, !CanHaveContent(html));
            }

            string tagName = html.Substring(0, spaceIndex);
            html = html.Substring(spaceIndex + 1);

            if (isClosed)
                html = html.Substring(0, html.Length - 1);

            if (!isClosed && !CanHaveContent(tagName.ToLower()))
                isClosed = true;

            HtmlElement element = new HtmlElement(tagName, isClosed);

            int len = html.Length;
            string attrName = string.Empty;
            string attrVal = string.Empty;
            bool isName = true;
            bool isValue = false;
            bool hasQuotes = false;

            for (var i = 0; i < len; i++)
            {
                string s = html.Substring(i, 1);

                if (s == "=" && !isValue)
                {
                    isName = false;
                    isValue = true;

                    string nextChar = html.Substring(i + 1, 1);
                    hasQuotes = (nextChar == "\"" || nextChar == "'");
                }
                else if (s == " " && isName)
                {
                    //add attribute that requires no value
                    element.Add(attrName, attrName);

                    //reset attribute name
                    attrName = string.Empty;
                    attrVal = string.Empty;
                }
                else if (s == " " && attrVal.Length > 0)
                {
                    if (!hasQuotes || (attrVal[0] == attrVal[attrVal.Length - 1]))
                    {
                        isValue = false;
                        isName = true;

                        string value = FixAttributeValue(attrVal);
                        element.Add(attrName, value);

                        attrName = string.Empty;
                        attrVal = string.Empty;
                    }
                    else if (isValue)
                        attrVal += s;
                }

                else if (isName)
                {
                    attrName += s;
                }
                else if (isValue)
                    attrVal += s;
            }

            if (isName && attrName != string.Empty && attrVal == string.Empty)
                attrVal = attrName;

            if (attrName != string.Empty && attrVal != string.Empty)
            {
                string value = FixAttributeValue(attrVal);
                element.Add(attrName, value);
            }

            return element;
        }

        private static bool CanHaveContent(string tagName)
        {
            return !"|img|br|input|hr|meta|base|link".Contains("|" + tagName + "|");
        }

        private static string FixAttributeValue(string attrValue)
        {
            if (attrValue.StartsWith("\"") && attrValue.EndsWith("\""))
                attrValue = attrValue.Substring(1, attrValue.Length - 2);
            else if (attrValue.StartsWith("'") && attrValue.EndsWith("'"))
                attrValue = attrValue.Substring(1, attrValue.Length - 2).Replace("\"", "&quot;");
            return attrValue;
        }

        private static string RemoveSpaces(string html)
        {
            html = html.Trim();

            var buf = new StringBuilder();
            int len = html.Length;
            string lastChar = string.Empty;
            bool withinTag = false;
            bool closingTag = false;
            string tagName = string.Empty;
            bool isAttribute = false;
            string quoteOpen = string.Empty;

            for (var i = 0; i < len; i++)
            {
                var s = html.Substring(i, 1);
                if (s == "<")
                {
                    withinTag = true;
                }
                else if (s == ">")
                {
                    withinTag = false;
                    closingTag = false;
                    tagName = "";
                    isAttribute = false;
                }
                else if (s == "/" && withinTag)
                    closingTag = true;

                if (withinTag)
                {
                    if (s == "\t" || s == "\n" || s == "\r")
                        s = " ";

                    if (s == " ")
                    {
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            isAttribute = true;
                            tagName = "";
                        }

                        if (lastChar == " " || lastChar == "/" || lastChar == "<" || lastChar == "=")
                            continue;

                        if (i < len + 1)
                        {
                            string nextChar = html.Substring(i + 1, 1);
                            if (nextChar == ">" || nextChar == "/" || nextChar == "\"" || nextChar == "=")
                                continue;
                        }
                    }
                    else
                    {
                        if (quoteOpen == string.Empty)
                        {
                            if (s == "\"" || s == "'")
                                quoteOpen = s;
                        }
                        else if (quoteOpen == s)
                        {
                            quoteOpen = string.Empty;
                            if (i < len + 1)
                            {
                                string nextChar = html.Substring(i + 1, 1);
                                if (nextChar != " ")
                                    s += " ";
                            }
                        }
                    }

                    if (!isAttribute)
                        tagName += s;
                }

                buf.Append(s);
                lastChar = s;
            }
            return buf.ToString();
        }
    }
}
