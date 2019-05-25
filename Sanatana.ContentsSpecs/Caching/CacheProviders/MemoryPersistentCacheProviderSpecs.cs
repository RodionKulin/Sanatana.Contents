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
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using FluentAssertions;

namespace Sanatana.ContentsSpecs.Caching.CacheProviders
{
    public class MemoryPersistentCacheProviderSpecs 
    {
        [TestFixture]
        public class when_adding : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            int _expectedValue = 1;
            bool _added;

            protected override void When()
            {
                _added = SUT.Add("key1", _expectedValue).Result;
            }

            [Test]
            public void then_should_add_successfuly()
            {
                _added.ShouldBeTrue();
            }

            [Test]
            public void then_does_not_add_entry_with_already_present_key()
            {
                int newValue = 2;
                bool added = SUT.Add("key1", newValue).Result;
                added.ShouldBeFalse();
            }

            [Test]
            public void then_returns_added_value()
            {
                int actualValue = SUT.Get<int>("key1").Result;
                actualValue.ShouldEqual(_expectedValue);
            }
        }

        [TestFixture]
        public class when_setting : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            int _newValue;

            protected override void When()
            {
                int originalValue = 1;
                SUT.Set("key4", originalValue).Wait();

                _newValue = 2;
                SUT.Set("key4", _newValue).Wait();
            }

            [Test]
            public void then_returns_latest_set_value()
            {
                int actualValue = SUT.Get<int>("key4").Result;
                actualValue.ShouldEqual(_newValue);
            }
        }

        [TestFixture]
        public class when_removing : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            protected override void Given()
            {
                int? value = 1;
                SUT.Set("key3", value).Wait();
            }

            protected override void When()
            {
                //remove existing value
                SUT.Remove("key3").Wait();
            }

            [Test]
            public void then_subsequence_remove_has_no_effect()
            {
                SUT.Remove("key3").Wait();
            }

            [Test]
            public void then_removed_value_is_not_found()
            {
                int? actualValue = SUT.Get<int?>("key3").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_removing_many : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            protected override void Given()
            {
                int value = 1;
                SUT.Set("key6", value).Wait();
                SUT.Set("key7", value).Wait();
            }

            protected override void When()
            {
                SUT.Remove(new List<string> { "key6", "key7" }).Wait();
            }

            [Test]
            public void then_subsequence_remove_has_no_effect()
            {
                SUT.Remove(new List<string> { "key6", "key7" }).Wait();
            }

            [Test]
            public void then_removed_values_are_not_found()
            {
                int? actualValue = SUT.Get<int?>("key6").Result;
                actualValue.ShouldBeNull();

                actualValue = SUT.Get<int?>("key7").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_removing_by_regex : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            int _value = 1;

            protected override void Given()
            {
                SUT.Set("regkey1", _value).Wait();
                SUT.Set("regkey2", _value).Wait();
                SUT.Set("regkey3", _value).Wait();
                SUT.Set("regkey4", _value).Wait();
            }

            protected override void When()
            {
                SUT.RemoveByRegex("regkey[1-3]").Wait();
            }

            [Test]
            public void then_removed_by_regex_values_are_not_found()
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
                remainingValue.ShouldEqual(_value);
            }
        }

        [TestFixture]
        public class when_clearing_cache : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            protected override void Given()
            {
                int value = 1;
                SUT.Set("key8", value).Wait();
                SUT.Set("key9", value).Wait();
            }

            protected override void When()
            {
                SUT.Clear().Wait();
            }

            [Test]
            public void then_clears_all_cached_values()
            {
                int? actualValue = SUT.Get<int?>("key8").Result;
                actualValue.ShouldBeNull();

                actualValue = SUT.Get<int?>("key9").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_removing_dependency_parent : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            bool _parentAdded;
            bool _childAdded;


            protected override void Given()
            {
                int value = 1;
                _parentAdded = SUT.Add("parent1", value).Result;

                _childAdded = SUT.Add("childKey", value, dependencyKeys: new List<string>()
                {
                    "parent1",
                    "parent2"
                }).Result;
            }

            protected override void When()
            {
                SUT.Remove("parent1").Wait();
            }

            [Test]
            public void then_values_are_initially_added()
            {
                _childAdded.ShouldBeTrue();
                _parentAdded.ShouldBeTrue();
            }

            [Test]
            public void then_removes_dependent_child_entry_on_parent_removed()
            {
                int? actualValue = SUT.Get<int?>("childKey").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_seting_dependency_parent : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            bool _childAdded;


            protected override void Given()
            {
                _childAdded = SUT.Add("childKey", 1, dependencyKeys: new List<string>()
                {
                    "parent1",
                    "parent2"
                }).Result;
            }

            protected override void When()
            {
                SUT.Set("parent1", 2).Wait();
            }

            [Test]
            public void then_values_are_initially_added()
            {
                _childAdded.ShouldBeTrue();
            }

            [Test]
            public void then_removes_dependent_child_entry_on_parent_set()
            {
                int? actualValue = SUT.Get<int?>("childKey").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_removing_dependency_chain : SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            bool _childAdded1;
            bool _childAdded2;
            bool _childAdded3;

            protected override void Given()
            {
                _childAdded1 = SUT.Add("childKey1", 1, dependencyKeys: new List<string>()
                {
                }).Result;

                _childAdded2 = SUT.Add("childKey2", 1, dependencyKeys: new List<string>()
                {
                    "childKey1"
                }).Result;

                _childAdded3 = SUT.Add("childKey3", 1, dependencyKeys: new List<string>()
                {
                    "childKey2"
                }).Result;
            }

            protected override void When()
            {
                SUT.Remove("childKey1").Wait();
            }

            [Test]
            public void then_values_are_initially_added()
            {
                _childAdded1.ShouldBeTrue();
                _childAdded2.ShouldBeTrue();
                _childAdded3.ShouldBeTrue();
            }

            [Test]
            public void then_removes_dependent_child_entry_on_parent_set()
            {
                int? actualValue = SUT.Get<int?>("childKey1").Result;
                actualValue.ShouldBeNull();

                actualValue = SUT.Get<int?>("childKey2").Result;
                actualValue.ShouldBeNull();

                actualValue = SUT.Get<int?>("childKey3").Result;
                actualValue.ShouldBeNull();
            }
        }

        [TestFixture]
        public class when_concurrently_adding_to_cache : 
            SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            Random _random = new Random(DateTime.Now.Millisecond);
            ConcurrentQueue<int?> _actualValues = new ConcurrentQueue<int?>();
            List<int?> _expectedValues;
            ConcurrentQueue<int?> _valuesQueue;

            protected override void Given()
            {
                _expectedValues = Enumerable.Range(0, 250)
                    .Select(x => (int?)x)
                    .ToList();

                _valuesQueue = new ConcurrentQueue<int?>(_expectedValues);
            }

            protected override void When()
            {
                var tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;

                SUT.Set("concurrent1", 0);

                List<Task> queryTasks = Enumerable.Range(0, 3)
                    .Select(x => Task.Factory.StartNew(stateObject =>
                    {
                        var castedToken = (CancellationToken)stateObject;
                        int? lastValue = null;
                        while (!token.IsCancellationRequested)
                        {
                            int? cacheValue = SUT.Get<int?>("concurrent1").Result;
                            if(lastValue != cacheValue)
                            {
                                _actualValues.Enqueue(cacheValue);
                            }
                        }
                    }, token, token))
                    .ToList();

                List<Task> commandTasks = Enumerable.Range(0, 3)
                    .Select(x => Task.Factory.StartNew(() =>
                    {
                        int? next = null;
                        while (_valuesQueue.TryDequeue(out next))
                        {
                            SUT.Set("concurrent1", next).Wait();
                        }
                        tokenSource.Cancel();
                    }))
                    .ToList();

                queryTasks.AddRange(commandTasks);
                Task.WaitAll(queryTasks.ToArray());
            }

            [Test]
            public void then_query_results_are_sorted_as_expected()
            {
                List<int> notNullValues = _actualValues
                    .Select(x => (int)x)
                    .Distinct()
                    .ToList();
                notNullValues.Should().BeInAscendingOrder("According to expected generated values");
            }
        }

        [TestFixture]
        public class when_concurrently_removing_from_cache : 
            SpecsFor<MemoryPersistentCacheProvider>, ICacheDependencies
        {
            ConcurrentQueue<int?> _valuesQueue;

            protected override void Given()
            {
                List<int?> expectedValues = Enumerable.Range(0, 250)
                    .Select(x => (int?)x)
                    .ToList();

                _valuesQueue = new ConcurrentQueue<int?>(expectedValues);
            }

            [Test]
            public void then_all_operations_succeed()
            {
                var tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;

                List<Task> removeTasks = Enumerable.Range(0, 3)
                    .Select(x => Task.Factory.StartNew(stateObject =>
                    {
                        var castedToken = (CancellationToken)stateObject;
                        while (!token.IsCancellationRequested)
                        {
                            SUT.Remove("concurrent1").Wait();
                        }
                    }, token, token))
                    .ToList();

                List<Task> setTasks = Enumerable.Range(0, 3)
                    .Select(x => Task.Factory.StartNew(() =>
                    {
                        int? next = null;
                        while (_valuesQueue.TryDequeue(out next))
                        {
                            SUT.Set("concurrent1", next).Wait();
                        }
                        tokenSource.Cancel();
                    }))
                    .ToList();

                removeTasks.AddRange(setTasks);
                Task.WaitAll(removeTasks.ToArray());
            }
        }

    }
}
