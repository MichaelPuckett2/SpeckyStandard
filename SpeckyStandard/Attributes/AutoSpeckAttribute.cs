using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AutoSpeckAttribute : Attribute
    {
        public InjectionMode InjectionMode { get; }
        public AutoSpeckAttribute(InjectionMode injectionMode = InjectionMode.Singleton) => InjectionMode = injectionMode;
    }
}
