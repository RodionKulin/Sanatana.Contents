using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.AmazonS3Files
{
    public class AmazonS3Settings
    {
        //свойства
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string RegionEndpoint { get; set; }
        public string BucketName { get; set; }
        public string BucketDomain { get; set; }


        //инициализация
        public static AmazonS3Settings FromConfig()
        {
            return new AmazonS3Settings()
            {
                AccessKey = ConfigurationManager.AppSettings["AWSAccessKey"],
                SecretKey = ConfigurationManager.AppSettings["AWSSecretKey"],
                RegionEndpoint = ConfigurationManager.AppSettings["AWSRegion"],
                BucketName = ConfigurationManager.AppSettings["AWSBucketName"],
                BucketDomain = ConfigurationManager.AppSettings["AWSBucketDomain"]
            };
        }
    }
}
