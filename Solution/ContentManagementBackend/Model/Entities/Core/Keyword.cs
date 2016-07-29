using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class Keyword<TKey>
        where TKey : struct
    {
        public TKey KeywordID { get; set; }
        public TKey ContentID { get; set; }
        public string Text { get; set; }
        public int Frequency { get; set; }
        public int Order { get; set; }


    }
}
