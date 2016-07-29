using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class HtmlInstruction : HtmlText
    {
        public HtmlInstruction(string str)
            : base(str)
        { }

        public override string ToString()
        {
            return string.Format("<!{0}>", base.ToString());
        }
    }
}
