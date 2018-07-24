using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Injects a class as a Speck dependency
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SpeckAttribute : Attribute
    {
        public Instantiation InjectionMode { get; }
        public Type ReferencedType { get; }

        public SpeckAttribute(Instantiation injectionMode = Instantiation.Singleton)
        {
            InjectionMode = injectionMode;
            ReferencedType = null;
        }

        public SpeckAttribute(Type referencedType)
        {
            InjectionMode = Instantiation.Singleton;
            ReferencedType = referencedType;
        }
    }
}
