using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Used on properties, fields, and parameters to auto initialize values based on existing Speck dependencies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AutoSpeckAttribute : Attribute { }
}
