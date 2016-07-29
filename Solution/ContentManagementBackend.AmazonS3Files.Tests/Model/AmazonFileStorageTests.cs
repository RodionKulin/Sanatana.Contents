using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContentManagementBackend.AmazonS3Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using System.Configuration;
using System.IO;

namespace ContentManagementBackend.AmazonS3Files.Tests
{
    [TestClass()]
    public class AmazonFileStorageTests
    {
        [TestMethod()]
        public void CreateTest()
        {
            ICommonLogger logger = new ShoutExceptionLogger();
            AmazonS3Settings settings = AmazonS3Settings.FromConfig();
            AmazonFileStorage target = new AmazonFileStorage(logger, settings);
            byte[] fileBytes = File.ReadAllBytes("Content/test2.jpg");
            string pathName = "test.jpg";

            bool result = target.Create(pathName, fileBytes).Result;
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void GetListTest()
        {
            ICommonLogger logger = new ShoutExceptionLogger();
            AmazonS3Settings settings = AmazonS3Settings.FromConfig();
            AmazonFileStorage target = new AmazonFileStorage(logger, settings);
            string folderPath = "posts-temp";

            QueryResult<List<FileDetails>> filesResult = target.GetList(folderPath).Result;
            Assert.IsFalse(filesResult.HasExceptions);
        }
        
    }
}