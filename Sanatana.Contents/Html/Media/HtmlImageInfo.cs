using Sanatana.Contents.Html.HtmlNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Html.Media
{
    public class HtmlImageInfo
    {
        //fields
        private HtmlAttribute _src;



        //properties
        public HtmlElement Element { get; set; }
        public string Src
        {
            get
            {
                return _src == null
                    ? string.Empty
                    : _src.Value ?? string.Empty;
            }
            set
            {
                if (_src == null)
                    return;

                _src.Value = value;
            }
        }



        //init
        public HtmlImageInfo()
        {

        }
        public HtmlImageInfo(HtmlElement element)
        {
            Element = element;

            _src = element.Attributes.FirstOrDefault(p => p.Name == "src");

        }


        
    }
}
