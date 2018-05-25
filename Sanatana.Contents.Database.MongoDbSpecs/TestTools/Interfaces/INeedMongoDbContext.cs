using Sanatana.Contents.Database.MongoDb;
using Sanatana.Contents.Database.MongoDb.Context;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database.MongoDbSpecs.TestTools.Interfaces
{
    public interface INeedMongoDbContext : ISpecs
    {
        IContentMongoDbContext MongoDbContext { get; set; }
    }
}
