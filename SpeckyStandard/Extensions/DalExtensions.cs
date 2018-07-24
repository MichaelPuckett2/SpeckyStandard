using SpeckyStandard.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpeckyStandard.Extensions
{
    internal static class DalExtensions
    {
        internal static BindingFlags BindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static List<(PropertyInfo propertyInfo, T dal)> GetDalProperties<T>(this Type speckType) where T : ContextBaseAttribute
        {
            return speckType
                  .GetProperties(BindingFlag)
                  .Select(prop => (prop, prop.GetCustomAttribute<T>()))
                  .Where(a => a.Item2 != null)
                  .ToList();
    }
}
}
