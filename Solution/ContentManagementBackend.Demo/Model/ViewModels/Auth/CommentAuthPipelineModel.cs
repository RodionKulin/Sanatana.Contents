using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class CommentAuthPipelineModel
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public HttpContext HttpContext { get; set; }
    }
}