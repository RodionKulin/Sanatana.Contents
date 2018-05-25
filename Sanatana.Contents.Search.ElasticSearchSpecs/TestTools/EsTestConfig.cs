using NUnit.Framework;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Interfaces;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Providers;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[SetUpFixture]
public class EsTestConfig : SpecsForConfiguration
{
    public EsTestConfig()
    {
        WhenTesting<INeedDependencies>().EnrichWith<DependenciesProvider>();
        WhenTesting<INeedClient>().EnrichWith<RecreateIndexProvider>();
        WhenTesting<INeedClient>().EnrichWith<ClientProvider>();
    }
}
