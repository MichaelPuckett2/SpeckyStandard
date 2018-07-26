using SpeckyStandard.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpeckyStandard.Extensions
{
    internal static class DalExtensions
    {
        internal static List<(PropertyInfo propertyInfo, T dal)> GetDalProperties<T>(this Type speckType) where T : ContextBaseAttribute
        {
            return speckType
                  .GetProperties(Constants.BindingFlags)
                  .Select(prop => (prop, prop.GetCustomAttribute<T>()))
                  .Where(a => a.Item2 != null)
                  .ToList();
    }
}
}
