using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class SpeckConfigurationAutoAttribute : Attribute
    {
        public SpeckConfigurationAutoAttribute(string key) => Key = key;
        public string Key { get; }
    }
}
