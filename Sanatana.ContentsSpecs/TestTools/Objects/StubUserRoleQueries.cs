using MongoDB.Bson;
using Sanatana.Contents.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.ContentsSpecs.TestTools.Objects
{
    public class StubUserRoleQueries : IUserRolesQueries<ObjectId>
    {
        public Task<List<ObjectId>> SelectUserRoles(ObjectId? userId)
        {
            return Task.FromResult(new List<ObjectId>());
        }
    }
}
