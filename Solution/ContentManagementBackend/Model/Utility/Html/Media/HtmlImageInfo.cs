using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class HtmlImageInfo
    {
        //поля
        private HtmlAttribute _src;



        //свойства
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



        //инициализация
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
