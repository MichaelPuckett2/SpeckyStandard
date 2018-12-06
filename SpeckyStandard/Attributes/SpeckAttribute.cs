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
        /// <summary>
        /// Describes whether the Speck will be a single instance or return a new instance for every injection.
        /// </summary>
        public SpeckType SpeckType { get; }

        /// <summary>
        /// A reference type used when injecting Specks. 
        /// ex: [Speck(typeof(ITestType))] public class Test ... will tell Specky to inject Test into all [SpeckAuto] decorated ITesetType's
        /// </summary>
        public Type ReferencedType { get; }

        /// <summary>
        /// Speck with optional SpeckType
        /// </summary>
        /// <param name="speckType">The SpeckType used for the Speck</param>
        public SpeckAttribute(SpeckType speckType = SpeckType.Singleton)
        {
            SpeckType = speckType;
            ReferencedType = null;
        }

        /// <summary>
        /// Speck with alternate reference type.
        /// Alternate references force the Speck to always be SpeckType.Singleton
        /// </summary>
        /// <param name="referencedType">The alternate type to reference the Speck</param>
        public SpeckAttribute(Type referencedType)
        {
            SpeckType = SpeckType.Singleton;
            ReferencedType = referencedType;
        }
    }
}
