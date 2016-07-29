using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.InitializerModules
{
    public class PostEssentials
    {
        public string Title { get; set; }
        public string ShortContent { get; set; }
        public string FullContent { get; set; }
        public FileInfo ImageFile { get; set;}
    }
}
