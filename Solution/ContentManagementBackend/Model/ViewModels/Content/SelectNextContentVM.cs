using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class SelectNextContentVM<TKey>
        where TKey : struct
    {
        //свойства
        public List<ContentRenderVM<TKey>> ContentRenderVMs { get; set; }
        public string PickID { get; set; }
        public bool IsContinued { get; set; }
        public string Error { get; set; }



        //инициализация
        public SelectNextContentVM()
        {

        }
        public SelectNextContentVM(string error)
        {
            Error = error;
        }
    }
}
