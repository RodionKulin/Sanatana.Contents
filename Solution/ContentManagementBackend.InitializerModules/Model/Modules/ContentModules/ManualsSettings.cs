using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.InitializerModules
{
    public class ManualsSettings
    {
        public ObjectId CategoryID { get; set; }
        public string PostContentPath { get; set; }
        public UserEssentials Author { get; set; }
    }
}
