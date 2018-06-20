using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpeckAttribute : Attribute
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
