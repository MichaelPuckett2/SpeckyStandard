namespace SpeckyStandard.Enums
{
    /// <summary>
    /// Tells Specky how to handle configuration logic.
    /// </summary>
    public enum ConfigurationMode
    {
        /// <summary>
        /// The configuration name is ignored.
        /// </summary>
        IgnoreConfiguration,
        /// <summary>
        /// The configuration name must match.
        /// </summary>
        ExactConfiguration,
        /// <summary>
        /// The configuration name cannot match.
        /// </summary>
        AllExceptConfiguration
    }
}
