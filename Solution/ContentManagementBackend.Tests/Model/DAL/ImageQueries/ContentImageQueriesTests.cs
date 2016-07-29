using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContentManagementBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ContentManagementBackend.Tests
{
    [TestClass()]
    public class ImageManagerTests
    {
        [TestMethod()]
        public void ExtractFileIDFromTempTest()
        {
            ContentImageQueries target = GetQueries();
            string url = string.Format("http://amazonaws.com/test/art-content-tmp/{0}{1}.jpeg"
                , "s10XS_KS_Umrlj8Gghl0SA"
                , "sw9Fb_0X3UmX_i5K2-dIJA");
            
            Guid? fileID = target.PathCreator.ExtractFileIDFromTemp(url);
            Assert.IsNotNull(fileID);
        }

        [TestMethod()]
        public void CleanTempTest()
        {
            ContentImageQueries target = GetQueries();
            
            bool completed = target.CleanTemp().Result;
            Assert.IsTrue(completed);
        }



        //common
        private ContentImageQueries GetQueries()
        {
            var settings = AmazonS3Files.AmazonS3Settings.FromConfig();
            IFileStorage storage = new AmazonS3Files.AmazonFileStorage(null, settings);

            var contentImageSettings = new ImageSettings()
            {
                Name = Constants.IMAGE_SETTINGS_NAME_CONTENT,
                TempDeleteAge = TimeSpan.FromDays(1),
                Targets = new List<ImageTargetParameters>()
                {
                    new ImageTargetParameters()
                    {
                        PathCreator = new PathCreator()
                        {
                            UrlBase = storage.GetBaseUrl(),
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_CONTENT,
                            TempNameFormat = "{0}{1}.jpeg"
                        }
                    }
                }
            };

            var factory = new ImageSettingsFactory(new List<ImageSettings>() { contentImageSettings });
            return new ContentImageQueries(storage, factory);
        }
    }
}