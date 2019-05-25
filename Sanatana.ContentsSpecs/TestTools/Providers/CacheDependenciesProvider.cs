using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Sanatana.ContentsSpecs.TestTools.Interfaces;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.ContentsSpecs.TestTools.Providers
{
    public class CacheDependenciesProvider : Behavior<ICacheDependencies>
    {
        public override void SpecInit(ICacheDependencies instance)
        {
            var options = Options.Create(new MemoryCacheOptions());
            instance.MockContainer.Configure(cfg =>
            {
                cfg.For<IMemoryCache>().Use(
                    new MemoryCache(Options.Create(new MemoryCacheOptions())));
            });

        }
    }
}
