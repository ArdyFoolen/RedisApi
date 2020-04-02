using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisApi.Common;
using RedisApi.Helpers;
using RedisApi.Tests.Helpers;

namespace RedisApi.Tests
{
    [TestClass]
    public class RedisCacheProviderTests
    {
        IRedisCacheProviderAsync _cacheProvider;
        List<Person> people;

        [TestInitialize]
        public void Initialize()
        {
            _cacheProvider = new RedisCacheProvider(new JsonSerializer());

            people = new List<Person>()
            {
            new Person(13, "Joe", new List<Contact>()
            {
                new Contact("111", "123456789"),
                new Contact("211", "234567890")
            })
            };
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _cacheProvider.RemoveAsync("People");
        }

        [TestMethod]
        public async Task Test_SetValue()
        {
            // Act
            await _cacheProvider.SetAsync("People", people);

            // Assert
            Assert.IsTrue(await _cacheProvider.KeyExistsAsync("People"));
        }

        [TestMethod]
        public async Task Test_SetValueWithExpire()
        {
            // Arrange
            TimeSpan timeSpan = new TimeSpan(0, 0, 10);

            // Act
            await _cacheProvider.SetAsync("People", people, timeSpan);

            // Assert
            Assert.IsTrue(await _cacheProvider.KeyExistsAsync("People"));
            Thread.Sleep(10000);
            Assert.IsFalse(await _cacheProvider.KeyExistsAsync("People"));
        }

        [TestMethod]
        public async Task Test_GetValue()
        {
            // Arrange
            await _cacheProvider.SetAsync("People", people);

            // Act
            var expected = await _cacheProvider.GetAsync<List<Person>>("People");

            Assert.IsNotNull(expected);
            Assert.AreEqual(people.Count, expected.Count);
            Assert.IsTrue(people.TrueForAll(p => p.Name.Equals(expected.Find(f => p.Id.Equals(f.Id)).Name)));

            Assert.IsTrue(people.TrueForAll(p => p.Contacts.Count == expected.Find(f => p.Id.Equals(f.Id)).Contacts.Count));
            Assert.IsTrue(people.TrueForAll(p => p.Contacts.
                TrueForAll(c => c.Value.Equals(expected.Find(f => p.Id.Equals(f.Id)).Contacts.Find(f => c.Type.Equals(f.Type)).Value))));
        }

        [TestMethod]
        public async Task TestIsInCache()
        {
            // Arrange
            await _cacheProvider.SetAsync("People", people);
            Thread.Sleep(1000);

            // Assert
            Assert.IsTrue(await _cacheProvider.KeyExistsAsync("People"));
        }

        [TestMethod]
        public async Task TestRemove()
        {
            // Arrange
            await _cacheProvider.SetAsync("People", people);
            Thread.Sleep(1000);

            // Act
            Assert.IsTrue(await _cacheProvider.KeyExistsAsync("People"));
            await _cacheProvider.RemoveAsync("People");

            // Assert
            Thread.Sleep(1000);
            Assert.IsFalse(await _cacheProvider.KeyExistsAsync("People"));
        }

        [TestMethod]
        public async Task NoTest_JustAddSomeData()
        {
            var testPeople = new List<Person>()
            {
                new Person(13, "Joe", new List<Contact>()
                {
                    new Contact("111", "123456789"),
                    new Contact("211", "234567890")
                }),
                new Person(14, "Bill", new List<Contact>()
                {
                    new Contact("311", "Home"),
                    new Contact("411", "Work")
                }),
                new Person(17, "Jack", new List<Contact>()
                {
                    new Contact("321", "Home Phone"),
                    new Contact("416", "Work Cell")
                }),
                new Person(19, "Rene", new List<Contact>()
                {
                    new Contact("637", "Extension"),
                    new Contact("876", "Mobile")
                }),
                new Person(23, "Chris", new List<Contact>()
                {
                    new Contact("574", "Ext"),
                    new Contact("762", "Mob123")
                })
            };
            await _cacheProvider.SetAsync("TestPeople", testPeople);
        }

        [TestMethod]
        public async Task Test_Subscribe()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            bool gotPublished = false;

            await provider.SubscribeAsync("SubscribeChannel", (channel, message) => gotPublished = true);
            await provider.PublishAsync("SubscribeChannel", "Message");

            Thread.Sleep(3000);
            Assert.IsTrue(gotPublished);
            await provider.UnsubscribeAsync("SubscribeChannel");
        }

        [TestMethod]
        public async Task Test_SubscribePattern()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            bool gotPublished = false;

            await provider.SubscribePatternAsync("Subscribe*l", (channel, message) => gotPublished = true);
            await provider.PublishAsync("SubscribeChannel", "Message");

            Thread.Sleep(3000);
            Assert.IsTrue(gotPublished);
            await provider.UnsubscribeAsync("Subscribe*l");
        }

        [TestMethod]
        public async Task TestPipeline()
        {
            List<Contact> contacts = CreateContacts();

            Task writeTask1 = _cacheProvider.SetAsync("People", people);
            Task writeTask2 = _cacheProvider.SetAsync("Contacts", contacts);
            await Task.WhenAll(writeTask1, writeTask2);

            Task<List<Person>> readTask1 = _cacheProvider.GetAsync<List<Person>>("People");
            Task<List<Contact>> readTask2 = _cacheProvider.GetAsync<List<Contact>>("Contacts");
            await Task.WhenAll(readTask1, readTask2);

            var resultPeople = await readTask1;
            var resultContacts = await readTask2;

            Assert.IsNotNull(resultPeople);
            Assert.AreEqual(people.Count, resultPeople.Count);
            Assert.IsTrue(people.TrueForAll(p => p.Name.Equals(resultPeople.Find(f => p.Id.Equals(f.Id)).Name)));

            Assert.IsTrue(people.TrueForAll(p => p.Contacts.Count == resultPeople.Find(f => p.Id.Equals(f.Id)).Contacts.Count));
            Assert.IsTrue(people.TrueForAll(p => p.Contacts.
                TrueForAll(c => c.Value.Equals(resultPeople.Find(f => p.Id.Equals(f.Id)).Contacts.Find(f => c.Type.Equals(f.Type)).Value))));

            Assert.AreEqual(contacts.Count, resultContacts.Count);
            Assert.IsTrue(contacts.
                TrueForAll(c => c.Value.Equals(resultContacts.Find(f => c.Type.Equals(f.Type)).Value)));
        }

        private static List<Contact> CreateContacts()
        {
            return new List<Contact>()
            {
                new Contact("1", "Contact 1"),
                new Contact("2", "Contact 2"),
                new Contact("3", "Contact 3"),
                new Contact("4", "Contact 4"),
                new Contact("5", "Contact 5"),
                new Contact("6", "Contact 6")
            };
        }

        [TestMethod]
        public async Task Test_Batch()
        {
            List<Contact> contacts = CreateContacts();

            var provider = _cacheProvider as RedisCacheProvider;
            IBatchProvider batch = await provider.CreateBatch() as IBatchProvider;

            Task writeTask1 = batch.SetAsync("People", people);
            Task writeTask2 = batch.SetAsync("Contacts", contacts);

            batch.Execute();
            await Task.WhenAll(writeTask1, writeTask2);

            Task<List<Person>> readTask1 = batch.GetAsync<List<Person>>("People");
            Task<List<Contact>> readTask2 = batch.GetAsync<List<Contact>>("Contacts");

            batch.Execute();
            await Task.WhenAll(readTask1, readTask2);

            var resultPeople = await readTask1;
            var resultContacts = await readTask2;

            Assert.IsNotNull(resultPeople);
            Assert.AreEqual(people.Count, resultPeople.Count);
            Assert.IsTrue(people.TrueForAll(p => p.Name.Equals(resultPeople.Find(f => p.Id.Equals(f.Id)).Name)));

            Assert.IsTrue(people.TrueForAll(p => p.Contacts.Count == resultPeople.Find(f => p.Id.Equals(f.Id)).Contacts.Count));
            Assert.IsTrue(people.TrueForAll(p => p.Contacts.
                TrueForAll(c => c.Value.Equals(resultPeople.Find(f => p.Id.Equals(f.Id)).Contacts.Find(f => c.Type.Equals(f.Type)).Value))));

            Assert.AreEqual(contacts.Count, resultContacts.Count);
            Assert.IsTrue(contacts.
                TrueForAll(c => c.Value.Equals(resultContacts.Find(f => c.Type.Equals(f.Type)).Value)));
        }

        [TestMethod]
        public async Task TestSetAdd()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            var mySet = new List<Person>()
            {
                new Person(13, "Joe", new List<Contact>()
                {
                    new Contact("111", "123456789"),
                    new Contact("211", "234567890")
                }),
                new Person(14, "Bill", new List<Contact>()
                {
                    new Contact("311", "Home"),
                    new Contact("411", "Work")
                }),
                new Person(17, "Jack", new List<Contact>()
                {
                    new Contact("321", "Home Phone"),
                    new Contact("416", "Work Cell")
                }),
                new Person(19, "Rene", new List<Contact>()
                {
                    new Contact("637", "Extension"),
                    new Contact("876", "Mobile")
                }),
                new Person(23, "Chris", new List<Contact>()
                {
                    new Contact("574", "Ext"),
                    new Contact("762", "Mob123")
                })
            };

            foreach (Person person in mySet)
                await provider.SetAddAsync("MySet", person);
        }

        [TestMethod]
        public async Task TestInSet()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            var mySet = new List<int>()
            {
                123, 456
            };

            foreach (int item in mySet)
                await provider.SetAddAsync("MyIntSet", item.ToString());

            Assert.IsTrue(await provider.InSetAsync("MyIntSet", "123"));
            Assert.IsTrue(await provider.InSetAsync("MyIntSet", "456"));
            Assert.IsFalse(await provider.InSetAsync("MyIntSet", "789"));
        }

        [TestMethod]
        public async Task TestSortedSetAdd()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            var mySet = new List<Person>()
            {
                new Person(13, "Joe", new List<Contact>()
                {
                    new Contact("111", "123456789"),
                    new Contact("211", "234567890")
                }),
                new Person(14, "Bill", new List<Contact>()
                {
                    new Contact("311", "Home"),
                    new Contact("411", "Work")
                }),
                new Person(17, "Jack", new List<Contact>()
                {
                    new Contact("321", "Home Phone"),
                    new Contact("416", "Work Cell")
                }),
                new Person(19, "Rene", new List<Contact>()
                {
                    new Contact("637", "Extension"),
                    new Contact("876", "Mobile")
                }),
                new Person(23, "Chris", new List<Contact>()
                {
                    new Contact("574", "Ext"),
                    new Contact("762", "Mob123")
                })
            };

            foreach (Person person in mySet)
                await provider.SortedSetAddAsync("MySortedSet", (double)person.Id, person);
        }

        [TestMethod]
        public async Task TestSortedSetGet()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            var mySet = new List<Person>()
            {
                new Person(13, "Joe", new List<Contact>()
                {
                    new Contact("111", "123456789"),
                    new Contact("211", "234567890")
                }),
                new Person(14, "Bill", new List<Contact>()
                {
                    new Contact("311", "Home"),
                    new Contact("411", "Work")
                }),
                new Person(17, "Jack", new List<Contact>()
                {
                    new Contact("321", "Home Phone"),
                    new Contact("416", "Work Cell")
                }),
                new Person(19, "Rene", new List<Contact>()
                {
                    new Contact("637", "Extension"),
                    new Contact("876", "Mobile")
                }),
                new Person(23, "Chris", new List<Contact>()
                {
                    new Contact("574", "Ext"),
                    new Contact("762", "Mob123")
                })
            };

            foreach (Person person in mySet)
                await provider.SortedSetAddAsync("MySortedSet", (double)person.Id, person);

            Person actual = await provider.SortedSetGetAsync<Person>("MySortedSet", (double)19);

            Assert.IsTrue(actual.Name.Equals(mySet[3].Name));

            Assert.IsTrue(actual.Contacts.Count == mySet[3].Contacts.Count);
            Assert.IsTrue(actual.Contacts.
                TrueForAll(c => c.Value.Equals(mySet[3].Contacts.Find(f => c.Type.Equals(f.Type)).Value)));
        }

        [TestMethod]
        public async Task TestSortedSetRange()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            var mySet = new List<Person>()
            {
                new Person(13, "Joe", new List<Contact>()
                {
                    new Contact("111", "123456789"),
                    new Contact("211", "234567890")
                }),
                new Person(14, "Bill", new List<Contact>()
                {
                    new Contact("311", "Home"),
                    new Contact("411", "Work")
                }),
                new Person(17, "Jack", new List<Contact>()
                {
                    new Contact("321", "Home Phone"),
                    new Contact("416", "Work Cell")
                }),
                new Person(19, "Rene", new List<Contact>()
                {
                    new Contact("637", "Extension"),
                    new Contact("876", "Mobile")
                }),
                new Person(23, "Chris", new List<Contact>()
                {
                    new Contact("574", "Ext"),
                    new Contact("762", "Mob123")
                })
            };

            foreach (Person person in mySet)
                await provider.SortedSetAddAsync("MySortedSet", (double)person.Id, person);

            List<Person> actual = new List<Person>();
            actual.AddRange(await provider.SortedSetRangeAsync<Person>("MySortedSet"));

            Assert.IsNotNull(actual);
            Assert.AreEqual(mySet.Count, actual.Count);
            Assert.IsTrue(mySet.TrueForAll(p => p.Name.Equals(actual.Find(f => p.Id.Equals(f.Id)).Name)));

            Assert.IsTrue(mySet.TrueForAll(p => p.Contacts.Count == actual.Find(f => p.Id.Equals(f.Id)).Contacts.Count));
            Assert.IsTrue(mySet.TrueForAll(p => p.Contacts.
                TrueForAll(c => c.Value.Equals(actual.Find(f => p.Id.Equals(f.Id)).Contacts.Find(f => c.Type.Equals(f.Type)).Value))));
        }

        [TestMethod]
        public async Task TestSortedSetRemove()
        {
            var provider = _cacheProvider as RedisCacheProvider;
            var mySet = new List<Person>()
            {
                new Person(13, "Joe", new List<Contact>()
                {
                    new Contact("111", "123456789"),
                    new Contact("211", "234567890")
                }),
                new Person(14, "Bill", new List<Contact>()
                {
                    new Contact("311", "Home"),
                    new Contact("411", "Work")
                }),
                new Person(17, "Jack", new List<Contact>()
                {
                    new Contact("321", "Home Phone"),
                    new Contact("416", "Work Cell")
                }),
                new Person(19, "Rene", new List<Contact>()
                {
                    new Contact("637", "Extension"),
                    new Contact("876", "Mobile")
                }),
                new Person(23, "Chris", new List<Contact>()
                {
                    new Contact("574", "Ext"),
                    new Contact("762", "Mob123")
                })
            };

            foreach (Person person in mySet)
                await provider.SortedSetAddAsync("MySortedSet", (double)person.Id, person);

            long result = await provider.SortedSetRemoveAsync<Person>("MySortedSet", (double)19);

            Person actual = await provider.SortedSetGetAsync<Person>("MySortedSet", (double)19);

            Assert.AreEqual(1, result);
            Assert.IsNull(actual);
        }

    }
}
