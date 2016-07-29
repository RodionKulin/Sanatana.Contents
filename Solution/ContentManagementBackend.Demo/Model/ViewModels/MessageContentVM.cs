using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class MessageContentVM
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}