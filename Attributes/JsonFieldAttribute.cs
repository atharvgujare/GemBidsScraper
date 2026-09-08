using System;
using GemBidScraper.Attributes;

namespace GemBidScraper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonFieldAttribute : Attribute
    {
        public string Name { get; }

        public JsonFieldAttribute(string name)
        {
            Name = name;
        }
    }
}