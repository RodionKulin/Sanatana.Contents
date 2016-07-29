using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ContentSubmitVM
    {
        public string ContentID { get; set; }
        public string UpdateNonce { get; set; }
        public bool MatchUpdateNonce { get; set; }
        public ImageStatus ImageStatus { get; set; }
        public Guid? ImageID { get; set; }
    }
}
