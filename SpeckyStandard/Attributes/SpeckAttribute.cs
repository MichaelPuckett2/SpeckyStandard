using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Injects a class as a Speck dependency
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SpeckAttribute : Attribute
    {
        public InjectionMode InjectionMode { get; }
        public Type ReferencedType { get; }

        public SpeckAttribute(InjectionMode injectionMode = InjectionMode.Singleton)
        {
            InjectionMode = injectionMode;
            ReferencedType = null;
        }

        public SpeckAttribute(Type referencedType)
        {
            InjectionMode = InjectionMode.Singleton;
            ReferencedType = referencedType;
        }
    }
}
