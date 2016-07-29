using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContentManagementBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.Tests
{
    [TestClass()]
    public class PathCreatorTests
    {
        [TestMethod()]
        public void CreateNamePathFormatTest()
        {
            PathCreator target = new PathCreator();
            string result = target.CombineFormatStrings("{0}{1}", "{0}{1}");
            Assert.AreEqual("{0}{1}{2}{3}", result);
        }
    }
}