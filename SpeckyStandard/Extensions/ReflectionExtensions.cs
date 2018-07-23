using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpeckyStandard.Extensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Retrieves types that have attribute of type T.
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="assembly">The assembly to search.</param>
        /// <returns>IEnumerable<Type> of types that have the T attribute.</returns>
        public static IEnumerable<Type> TypesWithAttribute<T>(this Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttribute<T>(true) != null);
        }
    }
}
