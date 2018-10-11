using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpeckConfigurationAttribute : Attribute
    {
        public SpeckConfigurationAttribute(string profile = "") => Profile = profile;
        public string Profile { get; }
    }
}
