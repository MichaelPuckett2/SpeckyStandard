using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Used on properties, fields, and parameters to auto intialize values based on existing Speck dependencies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AutoSpeckAttribute : Attribute
    {
        public InjectionMode InjectionMode { get; }
        public AutoSpeckAttribute(InjectionMode injectionMode = InjectionMode.Singleton) => InjectionMode = injectionMode;
    }
}
