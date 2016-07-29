using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class ULoginAuthPipelineModel
    {
        public string Token { get; set; }
        public string Host { get; set; }
        public HttpContext HttpContext { get; set; }
    }
}