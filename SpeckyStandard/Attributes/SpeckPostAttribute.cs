﻿using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// Specky calls this method after initializing all specks in the order or least dependent first.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SpeckPostAttribute : Attribute
    {
        /// <summary>
        /// Constructor that allows order number for ordering multiple post methods in a single class.
        /// </summary>
        /// <param name="order"></param>
        public SpeckPostAttribute(int order = 0) => Order = order;
        public int Order { get; }
    }
}
