using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Sanatana.Contents;
using SpecsFor;
using StructureMap;
using Should;
using Sanatana.ContentsSpecs.TestTools.Interfaces;
using Sanatana.Contents.Caching.CacheProviders;

namespace Sanatana.ContentsSpecs.Caching.CacheProviders
{
    public class MemoryPersistentCacheProviderSpecs 
    {
        [TestFixture]
        public class when_adding : SpecsFor<MemoryPersistentCacheProvider> , ICacheDependencies
        {
            int value = 1;
            bool added;

            protected override void When()
            {
                added = SUT.Add("key1", value).Result;
                added.ShouldBeTrue();
            }

            [Test]
            public void then_does_not_add_entry_with_already_present_key()
            {
                //then_does_not_add_entry_with_already_present_key
                int newValue = 2;
                bool added = SUT.Add("key1", newValue).Result;
                added.ShouldBeFalse();

                //then_returns_added_value
                int actualValue = SUT.Get<int>("key1").Result;
                actualValue.ShouldEqual(value);
            }
        }

        [TestFixture]
        public class when_setting : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {

            [Test]
            public void then_returns_latest_set_value()
            {
                int value = 1;
                SUT.Set("key4", value).Wait();

                int newValue = 2;
                SUT.Set("key4", newValue).Wait();
                
                int actualValue = SUT.Get<int>("key4").Result;
                actualValue.ShouldEqual(newValue);
            }
        }

        [TestFixture]
        public class when_removing : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            protected override void When()
            {
                int value = 1;
                SUT.Set("key3", value).Wait();
            }

            [Test]
            public void then_removes_existing_value()
            {
                //remove existing value
                SUT.Remove("key3").Wait();
                int? actualValue = SUT.Get<int?>("key3").Result;
                actualValue.ShouldBeNull();

                //then_removes_non_existing_value
                SUT.Remove("key3").Wait();
                actualValue = SUT.Get<int?>("key3").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_removing_many : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            protected override void When()
            {
                int value = 1;
                SUT.Set("key6", value).Wait();
                SUT.Set("key7", value).Wait();
            }

            [Test]
            public void then_removes_many_existing_values()
            {
                //remove existing value
                SUT.Remove(new List<string> { "key6", "key7" }).Wait();

                int? actualValue = SUT.Get<int?>("key6").Result;
                actualValue.ShouldBeNull();
                actualValue = SUT.Get<int?>("key7").Result;
                actualValue.ShouldBeNull();

                //then_removes_non_existing_value
                SUT.Remove(new List<string> { "key6", "key7" }).Wait();

                actualValue = SUT.Get<int?>("key6").Result;
                actualValue.ShouldBeNull();
                actualValue = SUT.Get<int?>("key7").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_removing_by_regex : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            int value = 1;

            protected override void When()
            {
                SUT.Set("regkey1", value).Wait();
                SUT.Set("regkey2", value).Wait();
                SUT.Set("regkey3", value).Wait();
                SUT.Set("regkey4", value).Wait();

                SUT.RemoveByRegex("regkey[1-3]").Wait();
            }

            [Test]
            public void then_removed_by_regex_values_are_actually_removed()
            {
                int? removedValue = SUT.Get<int?>("regkey1").Result;
                removedValue.ShouldBeNull();

                removedValue = SUT.Get<int?>("regkey2").Result;
                removedValue.ShouldBeNull();

                removedValue = SUT.Get<int?>("regkey3").Result;
                removedValue.ShouldBeNull();
            }

            [Test]
            public void then_not_removed_by_regex_values_remain()
            {
                int? remainingValue = SUT.Get<int?>("regkey4").Result;
                remainingValue.ShouldEqual(value);
            }
        }

        [TestFixture]
        public class when_clearing_cache : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            protected override void When()
            {
                int value = 1;
                SUT.Set("key8", value).Wait();
                SUT.Set("key9", value).Wait();
            }

            [Test]
            public void then_clears_all_cached_values()
            {
                //remove existing value
                SUT.Clear().Wait();

                int? actualValue = SUT.Get<int?>("key8").Result;
                actualValue.ShouldBeNull();
                actualValue = SUT.Get<int?>("key9").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_setting_dependent : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            protected override void When()
            {
                int value = 1;
                bool added = SUT.Add("key2", value, dependFromKeys: new List<string>()
                {
                    "dep1",
                    "dep2"
                }).Result;
                added.ShouldBeTrue();

                added = SUT.Add("dep1", value).Result;
                added.ShouldBeTrue();

                SUT.Remove("dep1").Wait();
            }

            [Test]
            public void then_removes_dependent_entry_on_parent_removed()
            {
                int? actualValue = SUT.Get<int?>("key2").Result;
                actualValue.ShouldBeNull();
            }
        }
    }
}
