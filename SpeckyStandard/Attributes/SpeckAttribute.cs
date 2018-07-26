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
        public SpeckType SpeckType { get; }
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
        /// Alternate references forece the Speck to always be SpeckType.Singleton
        /// </summary>
        /// <param name="referencedType">The alternate type to reference the Speck</param>
        public SpeckAttribute(Type referencedType)
        {
            SpeckType = SpeckType.Singleton;
            ReferencedType = referencedType;
        }
    }
}
