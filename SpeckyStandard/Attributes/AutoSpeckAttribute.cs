using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Used on properties and fields to auto initialize values based on existing Speck dependencies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class AutoSpeckAttribute : Attribute
    {
        /// <summary>
        /// The target Speck used to initialize the AutoSpeck.
        /// </summary>
        public Type OfType { get; }

        /// <summary>
        /// Auto initializes the value with the appropriate Speck.
        /// </summary>
        /// <param name="ofType">Optional Speck target used to initialize the value.</param>
        public AutoSpeckAttribute(Type ofType = null)
        {
            OfType = ofType;
        }
    }
}
