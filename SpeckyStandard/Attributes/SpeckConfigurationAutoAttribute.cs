using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Sets the value of a type found via the current configuration based on the name of the property, field, parameter, or supplied key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class SpeckConfigurationAutoAttribute : Attribute
    {
        /// <summary>
        /// Initializes SpeckConfigurationAuto with optional configuration key used to lookup value.
        /// </summary>
        /// <param name="configurationKey">The configuration key used to look up the configured value. If left empty the name of the property, field, or parameter will be used as the key.</param>
        public SpeckConfigurationAutoAttribute(string configurationKey = "") => ConfigurationKey = configurationKey;

        /// <summary>
        /// The configuration key used to look up the configured value.
        /// </summary>
        public string ConfigurationKey { get; }
    }
}
