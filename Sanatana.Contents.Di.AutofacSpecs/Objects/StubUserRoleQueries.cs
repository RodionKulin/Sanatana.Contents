using MongoDB.Bson;
using Sanatana.Contents.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Di.AutofacSpecs.Objects
{
    public class StubUserRoleQueries : IUserRolesQueries<ObjectId>
    {
        public Task<List<ObjectId>> SelectUserRoles(ObjectId? userId)
        {
            throw new NotImplementedException();
        }
    }
}
