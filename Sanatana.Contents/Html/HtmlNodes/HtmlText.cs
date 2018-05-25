using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Html.HtmlNodes
{
    public class HtmlText : HtmlNode
    {
        private readonly string _text;

        public HtmlText(string text)
        {
            _text = text ?? string.Empty;
        }

        public override string ToString()
        {
            return _text;
        }
    }
}
