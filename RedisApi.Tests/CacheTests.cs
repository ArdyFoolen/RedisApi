//using System.Collections.Generic;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using RedisApi.Common;
//using RedisApi.Tests.Helpers;
//using System;
//using System.Text;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace RedisApi.Tests
//{
//    [TestClass]
//    public class CacheTests
//    {
//        ICacheProvider _cacheProvider;
//        List<Person> people;

//        [TestInitialize]
//        public void Initialize()
//        {
//            _cacheProvider = new RedisCacheProvider();

//            people = new List<Person>()
//            {
//            new Person(13, "Joe", new List<Contact>()
//            {
//                new Contact("111", "123456789"),
//                new Contact("211", "234567890")
//            })
//            };
//        }

//        [TestCleanup]
//        public void Cleanup()
//        {
//            _cacheProvider.Remove("People");
//        }

//        [TestMethod]
//        public void Test_SetValue()
//        {
//            // Act
//            _cacheProvider.Set("People", people);

//            // Assert
//            Assert.IsTrue(_cacheProvider.IsInCache("People"));
//        }

//        [TestMethod]
//        public void Test_SetValueWithExpire()
//        {
//            // Arrange
//            TimeSpan timeSpan = new TimeSpan(0, 0, 10);

//            // Act
//            _cacheProvider.Set("People", people, timeSpan);

//            // Assert
//            Assert.IsTrue(_cacheProvider.IsInCache("People"));
//            Thread.Sleep(10000);
//            Assert.IsFalse(_cacheProvider.IsInCache("People"));
//        }

//        [TestMethod]
//        public void Test_GetValue()
//        {
//            // Arrange
//            _cacheProvider.Set("People", people);

//            // Act
//            var expected = _cacheProvider.Get<List<Person>>("People");

//            Assert.IsNotNull(expected);
//            Assert.AreEqual(people.Count, expected.Count);
//            Assert.IsTrue(people.TrueForAll(p => p.Name.Equals(expected.Find(f => p.Id.Equals(f.Id)).Name)));

//            Assert.IsTrue(people.TrueForAll(p => p.Contacts.Count == expected.Find(f => p.Id.Equals(f.Id)).Contacts.Count));
//            Assert.IsTrue(people.TrueForAll(p => p.Contacts.
//                TrueForAll(c => c.Value.Equals(expected.Find(f => p.Id.Equals(f.Id)).Contacts.Find(f => c.Type.Equals(f.Type)).Value))));
//        }

//        [TestMethod]
//        public void TestIsInCache()
//        {
//            // Arrange
//            _cacheProvider.Set("People", people);

//            // Assert
//            Assert.IsTrue(_cacheProvider.IsInCache("People"));
//        }

//        [TestMethod]
//        public void TestRemove()
//        {
//            // Arrange
//            _cacheProvider.Set("People", people);

//            // Act
//            Assert.IsTrue(_cacheProvider.IsInCache("People"));
//            _cacheProvider.Remove("People");

//            // Assert
//             Assert.IsFalse(_cacheProvider.IsInCache("People"));
//        }

//        [TestMethod]
//        public void Test_GetKeysAndTypes()
//        {
//            var provider = _cacheProvider as RedisCacheProvider;
//            var keys = provider.Keys();

//            foreach (var key in keys)
//            {
//                string type = provider.Type(key);
//            }
//        }

//        [TestMethod]
//        public void NoTest_JustAddSomeData()
//        {
//            var testPeople = new List<Person>()
//            {
//                new Person(13, "Joe", new List<Contact>()
//                {
//                    new Contact("111", "123456789"),
//                    new Contact("211", "234567890")
//                }),
//                new Person(14, "Bill", new List<Contact>()
//                {
//                    new Contact("311", "Home"),
//                    new Contact("411", "Work")
//                }),
//                new Person(17, "Jack", new List<Contact>()
//                {
//                    new Contact("321", "Home Phone"),
//                    new Contact("416", "Work Cell")
//                }),
//                new Person(19, "Rene", new List<Contact>()
//                {
//                    new Contact("637", "Extension"),
//                    new Contact("876", "Mobile")
//                }),
//                new Person(23, "Chris", new List<Contact>()
//                {
//                    new Contact("574", "Ext"),
//                    new Contact("762", "Mob123")
//                })
//            };
//            _cacheProvider.Set("TestPeople", testPeople);
//        }
//    }
//}
