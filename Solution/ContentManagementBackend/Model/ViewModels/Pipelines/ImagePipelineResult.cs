using Common.Utility.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ImagePipelineResult
    {
        //свойства
        public string Url { get; set; }
        public Guid? FileID { get; set; }
    }
}
