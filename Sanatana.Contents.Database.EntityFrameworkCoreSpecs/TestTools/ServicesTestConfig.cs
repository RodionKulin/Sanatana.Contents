using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Providers;
using NUnit.Framework;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[SetUpFixture]
public class ServicesTestConfig : SpecsForConfiguration
{
    public ServicesTestConfig()
    {
        WhenTesting<INeedDatabase>().EnrichWith<DependenciesProvider>();
        WhenTesting<INeedDatabase>().EnrichWith<DatabaseCreator>();
        WhenTesting<INeedDatabase>().EnrichWith<DataPurger>();
    }
}
