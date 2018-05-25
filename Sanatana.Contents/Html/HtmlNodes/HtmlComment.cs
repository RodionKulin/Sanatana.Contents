using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Html.HtmlNodes
{
    public class HtmlComment : HtmlText
    {
        public HtmlComment(string comment)
            : base(comment)
        { }

        public override string ToString()
        {
            return string.Format("<!--{0}-->", base.ToString());
        }
    }
}
