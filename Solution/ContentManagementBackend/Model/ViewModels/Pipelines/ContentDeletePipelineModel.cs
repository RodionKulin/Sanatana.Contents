using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ContentDeletePipelineModel<TKey>
        where TKey : struct
    {
        public TKey ContentID { get; set; }
        public List<string> UserRoles { get; set; }
        public int Permission { get; set; }
    }
}
