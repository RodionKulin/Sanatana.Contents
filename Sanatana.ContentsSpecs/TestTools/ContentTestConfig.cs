using NUnit.Framework;
using Sanatana.ContentsSpecs.TestTools.Interfaces;
using Sanatana.ContentsSpecs.TestTools.Providers;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[SetUpFixture]
public class ContentTestConfig : SpecsForConfiguration
{
    public ContentTestConfig()
    {
        WhenTesting<ICacheDependencies>().EnrichWith<CacheDependenciesProvider>();
    }
}
