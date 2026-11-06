using System;
using OpenQA.Selenium;

namespace TALXIS.TestKit.Selectors.WebClientManagement.Helpers
{
    class SerializableCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public DateTime? Expiry { get; set; }

        public SerializableCookie() { }

        public SerializableCookie(Cookie cookie)
        {
            Name = cookie.Name;
            Value = cookie.Value;
            Domain = cookie.Domain;
            Path = cookie.Path;
            Expiry = cookie.Expiry;
        }
    }
}
