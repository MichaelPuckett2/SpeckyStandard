using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Invokes method after all Specks are injected and after all strapping operations. Optionally a configuration name can be used to invoke only during that configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]

    public class SpeckyStrappedAttribute : Attribute
    {
        /// <summary>
        /// Tells Specky to invoke the method once all strapping is complete regardless of configuration.
        /// Defaults ConfigurationMode to IgnoreConfiguration.
        /// </summary>
        public SpeckyStrappedAttribute()
        {
            ConfigurationMode = ConfigurationMode.IgnoreConfiguration;
        }

        /// <summary>
        /// Tells Specky to invoke the method once all strapping is complete.
        /// </summary>
        /// <param name="configurationMode">The configuration mode used to determine if the method should be invoked.</param>
        /// <param name="configurationName">The configuration name used to determine if the method should be invoked.  Used in conjunction with ConfigurationMode</param>
        public SpeckyStrappedAttribute(ConfigurationMode configurationMode, string configurationName = "")
        {
            ConfigurationName = configurationName;
            ConfigurationMode = configurationMode;
        }

        /// <summary>
        /// Configuration used to invoke the SpeckyStrapped method.
        /// </summary>
        public string ConfigurationName { get; }

        /// <summary>
        /// The configuration mode used to determine whether or not the method is invoked.
        /// </summary>
        public ConfigurationMode ConfigurationMode { get; }
    }
}
