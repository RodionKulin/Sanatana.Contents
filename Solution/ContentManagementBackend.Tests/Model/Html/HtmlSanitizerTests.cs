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
    public class HtmlSanitizerTests
    {
        [TestMethod()]
        public void SanitizeTest()
        {
            string input = "<p><br/>Input&nbsp;text<br/></p>";
            
            HtmlSanitizer sanitize = new HtmlSanitizer();
            string result = sanitize.Sanitize(input);

            Assert.AreEqual(input, result);
        }
    }
}