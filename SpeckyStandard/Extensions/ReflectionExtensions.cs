using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpeckyStandard.Extensions
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> TypesWithAttribute<T>(this Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttribute<T>() != null);
        }
    }
}
