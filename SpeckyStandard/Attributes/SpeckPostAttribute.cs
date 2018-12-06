using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Specky calls this method after initializing all specks in the order of least dependent first and then by Order number if supplied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SpeckPostAttribute : Attribute
    {
        /// <summary>
        /// Constructor that allows order number for ordering multiple post methods in a single class.
        /// </summary>
        /// <param name="order"></param>
        public SpeckPostAttribute(int order = 0) => Order = order;

        /// <summary>
        /// The order in which Specky calls the post method per type from lowest to highest.
        /// </summary>
        public int Order { get; }
    }
}
