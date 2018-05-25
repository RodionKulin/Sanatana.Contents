using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Contents
{
    public class ContentDeleteParams<TKey>
        where TKey : struct
    {
        public TKey ContentId { get; set; }
        public long Version { get; set; }
        public bool CheckVersion { get; set; }
        public TKey? UserId { get; set; }
        public int Permission { get; set; }
        public int? ContentImagesPathMapperId { get; set; }

    }
}
