//http://www.codeproject.com/Tips/529712/Dead-Simple-HTML-Sanitizer
//https://www.owasp.org/index.php/XSS_Filter_Evasion_Cheat_Sheet#Image_XSS_using_the_JavaScript_directive
//Don't forget onclick, onmouseover, etc or javascript: psuedo-urls (<img src="javascript:evil!Evil!">) or CSS (style="property: expression(evil!Evil!);") or

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;


namespace ContentManagementBackend
{
    public class HtmlSanitizer
    {
        //поля
        private static readonly Collection<string> _unsafe;


        //свойства
        public List<string> TagsWhiteList { get; set; }
        public List<string> AllowedIFrameUrls { get; set; }
        

        //инициализация
        static HtmlSanitizer()
        {
            string[] list = new[]
            {
                "onload", "onerror", "onclick", "ondblclick", "onmousedown", "onmouseup", "onmouseover", "onmousemove",
                "onmouseout", "onkeypress", "onkeydown", "onkeyup", "script", "applet", "embed", "frameset",
                "object", "ilayer", "layer"
            };
            _unsafe = new Collection<string>();
           
            foreach (string item in list)
            {
                _unsafe.Add(item);
            }           
        }

        public HtmlSanitizer(List<string> tagsWhiteList = null, List<string> allowedIFrameUrls = null)
        {
            TagsWhiteList = tagsWhiteList
                ?? new List<string>() { "br", "span", "img", "p", "div", "strong", "em", "u", "s",
                "ol", "ul", "li", "table", "tbody", "tr", "td", "blockquote", "figure", "figcaption", "a", "iframe" };
            
            AllowedIFrameUrls = allowedIFrameUrls;
        }



        //методы
        public string Sanitize(string html)
        {
            HtmlRootElement doc = HtmlParser.Parse(html, true);
            Sanitize(doc);

            if (doc.ChildNodes.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                return doc.ToString();
            }
        }
        
        public void Sanitize(HtmlElement element)
        {
            IEnumerable<HtmlAttribute> attributesToRemove =
                from attribute in element.Attributes
                where IsUnSafe(attribute)
                select attribute;

            element.RemoveAll(attributesToRemove);

            for (var i = element.ChildNodes.Count - 1; i >= 0; i--)
            {
                HtmlNode childNode = element.ChildNodes[i];

                if (IsUnSafe(childNode)
                    || childNode is HtmlInstruction
                    || childNode is HtmlComment)
                {
                    element.ChildNodes.RemoveAt(i);
                }
            }

            foreach (HtmlNode node in element.ChildNodes)
            {
                if (node is HtmlElement)
                    Sanitize(node as HtmlElement);                
            }
        }

        private bool IsUnSafe(HtmlAttribute attribute)
        {
            if (_unsafe.Contains(attribute.Name))
                return true;

            if (!string.IsNullOrEmpty(attribute.Value))
            {
                string val = attribute.Value.TrimStart(new char[] { ' ' }).ToLowerInvariant();
                if (val.StartsWith("javascript"))
                    return true;
            }

            return false;
        }

        private bool IsUnSafe(HtmlNode node)
        {
            if (node is HtmlInstruction || node is HtmlComment)
                return true;

            if (node is HtmlElement)
            {
                HtmlElement element = node as HtmlElement;
                if (TagsWhiteList != null && !TagsWhiteList.Contains(element.Name))
                    return true;

                if (element.Name == "iframe" || element.Name == "frame")
                {
                    if (AllowedIFrameUrls == null)
                        return true;

                    HtmlAttribute src = element.Attributes.FirstOrDefault(p => p.Name == "src");
                    Uri address;
                    if (src == null || !Uri.TryCreate(src.Value, UriKind.Absolute, out address))
                        return true;

                    if (AllowedIFrameUrls.All(p => !Regex.IsMatch(address.Authority, p)))
                        return true;                    
                }

                if (_unsafe.Contains(element.Name))
                    return true;                
            }

            return false;
        }
    }
}
