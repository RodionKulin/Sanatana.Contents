using NUnit.Framework;
using Sanatana.Contents.Database.MongoDb;
using Sanatana.Contents.Database.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using MongoDB.Bson;
using System.Diagnostics;
using MongoDB.Driver;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Database.MongoDb.Queries;
using Sanatana.Contents.Objects.Entities;

namespace Sanatana.Contents.Database.MongoDbSpecs
{
    public class MongoDbCategoryRolePermissionQueriesSpecs
    {

        [TestFixture]
        public class when_selecting_category_role_permissions_bitwise : SpecsFor<MongoDbCategoryRolePermissionQueries>
            , INeedMongoDbContext
        {
            private enum TestPermission { Read = 1, Write = 2, Moderate = 4, Delete = 8 }
            private ObjectId _roleId;
            public IContentMongoDbContext MongoDbContext { get; set; }


            protected override void Given()
            {
                _roleId = ObjectId.GenerateNewId();
                var permissions = new[] {
                    1,  //Read
                    2,  //Write
                    5,  //Moderate, Read
                    6   //Moderate, Write
                }.Select(x => new CategoryRolePermission<ObjectId>
                {
                    CategoryRolePermissionId = ObjectId.GenerateNewId(),
                    RoleId = _roleId,
                    CategoryId = ObjectId.GenerateNewId(),
                    PermissionFlags = x
                }).ToList();

                MongoDbContext.GetCollection<CategoryRolePermission<ObjectId>>().InsertMany(permissions);
            }

            [Test]
            public void then_returns_permissions_using_bitwise_and_linq()
            {
                long permission = (long)TestPermission.Write;
                List<CategoryRolePermission<ObjectId>> permissions = SUT.SelectMany(
                    x => (x.PermissionFlags & permission) == permission)
                    .Result;
                
                permissions.Count.ShouldEqual(2);
            }

            [Test]
            public void then_returns_permissions_using_bitwise_and_bitsallset()
            {
                long permission = (long)TestPermission.Write;

                var filter = Builders<CategoryRolePermission<ObjectId>>.Filter
                    .BitsAllSet(x => x.PermissionFlags, permission);
                filter = Builders<CategoryRolePermission<ObjectId>>.Filter.And(filter,
                    Builders<CategoryRolePermission<ObjectId>>.Filter.Where(x => x.RoleId == _roleId));

                List<CategoryRolePermission<ObjectId>> permissions =
                    MongoDbContext.GetCollection<CategoryRolePermission<ObjectId>>().Find(filter).ToList();

                permissions.Count.ShouldEqual(2);
            }
        }

    }
}
