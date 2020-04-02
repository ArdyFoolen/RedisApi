using System;
using System.Collections.Generic;

namespace RedisApi.Tests.Helpers
{
    [Serializable]
    public class Person
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<Contact> Contacts { get; set; }

        private Person() { }
        public Person(long id, string name, List<Contact> contacts)
        {
            this.Id = id;
            this.Name = name;
            this.Contacts = contacts;
        }
    }
}
