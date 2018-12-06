using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpeckConfigurationAttribute : Attribute
    {
        public SpeckConfigurationAttribute(string configuration = "") => Configuration = configuration;
        public string Configuration { get; }
    }
}
