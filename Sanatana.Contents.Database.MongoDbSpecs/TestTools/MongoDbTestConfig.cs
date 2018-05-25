using NUnit.Framework;
using Sanatana.Contents.Database.MongoDbSpecs.TestTools.Interfaces;
using Sanatana.Contents.Database.MongoDbSpecs.TestTools.Providers;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[SetUpFixture]
public class MongoDbTestConfig : SpecsForConfiguration
{
    public MongoDbTestConfig()
    {
        WhenTesting<INeedMongoDbContext>().EnrichWith<DependenciesProvider>();
        WhenTesting<INeedMongoDbContext>().EnrichWith<DropDatabase>();
    }
}
