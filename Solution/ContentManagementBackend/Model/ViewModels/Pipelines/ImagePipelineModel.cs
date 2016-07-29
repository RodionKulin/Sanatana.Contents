using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class ImagePipelineModel
    {
        public Stream InputStream { get; set; }
        public long ContentLength { get; set; }
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
        public List<ImageTargetParameters> Targets { get; set; }
        public List<string> TargetNamePaths { get; set; }
        public long SizeLimit { get; set; }
    }
}
