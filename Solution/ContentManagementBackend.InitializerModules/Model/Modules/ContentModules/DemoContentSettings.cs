using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.InitializerModules
{
    public class DemoContentSettings
    {
        public bool CreatePreviewImages { get; set; }
        public bool MarkAsIndexedInSearch { get; set; }
        public ObjectId CategoryID { get; set; }
        public string PostContentPath { get; set; }
        public int PostCount { get; set; }
    }
}
