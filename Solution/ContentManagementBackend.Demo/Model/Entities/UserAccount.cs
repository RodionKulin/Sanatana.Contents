using Common.Identity2_1.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class UserAccount : MongoIdentityUser
    {
        public string Avatar { get; set; }
    }
}