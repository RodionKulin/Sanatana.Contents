using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Html.HtmlNodes
{
    public class HtmlRootElement : HtmlElement
    {
        public HtmlRootElement(string name, bool isClosed)
            : base(name, isClosed)
        {
        }

        public override string ToString()
        {
            string html = base.ToString();
            return html == "<html/>"
                ? string.Empty
                : html.Substring(6, html.Length - 13);
        }
    }
}
