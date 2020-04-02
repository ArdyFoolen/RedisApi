using System;

namespace RedisApi.Tests.Helpers
{
    [Serializable]
    public class Contact
    {
        public string Type { get; set; }

        public string Value { get; set; }

        private Contact() { }
        public Contact(string type, string value)
        {
            this.Type = type;
            this.Value = value;
        }
    }
}
