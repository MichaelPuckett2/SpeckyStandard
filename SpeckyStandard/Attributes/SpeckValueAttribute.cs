using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class SpeckValueAttribute : Attribute
    {
        public SpeckValueAttribute(string key) => Key = key;
        public string Key { get; }
    }
}
