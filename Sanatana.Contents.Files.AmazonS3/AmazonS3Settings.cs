using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.AmazonS3
{
    public class AmazonS3Settings
    {
        //properties
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string RegionEndpoint { get; set; }
        public string BucketName { get; set; }
        public string BucketDomain { get; set; }


    }
}
