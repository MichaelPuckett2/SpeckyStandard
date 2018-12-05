using SpeckyStandard.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpeckyStandard.Extensions
{
    internal static class SpeckExtensions
    {
        public static T GetAttribute<T>(this Type type, Predicate<T> when = null) where T : Attribute
        {
            if (when == null) when = (obj) => true;
            T customAttribute = (T)type.GetCustomAttribute(typeof(T));
            return when.Invoke(customAttribute) 
                 ? customAttribute 
                 : null;
        }

        public static bool TryGetAttribute<T>(this Type type, out T t, Predicate<T> when = null) where T : Attribute
        {
            t = type.GetAttribute(when);
            return t != null;
        }

        public static T GetAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return (T)propertyInfo.GetCustomAttribute(typeof(T));
        }

        public static bool TryGetAttribute<T>(this PropertyInfo propertyInfo, out T t) where T : Attribute
        {
            t = propertyInfo.GetAttribute<T>();
            return t != null;
        }

        public static T GetAttribute<T>(this FieldInfo fieldInfo) where T : Attribute
        {
            return (T)fieldInfo.GetCustomAttribute(typeof(T));
        }

        public static bool TryGetAttribute<T>(this FieldInfo fieldInfo, out T t) where T : Attribute
        {
            t = fieldInfo.GetAttribute<T>();
            return t != null;
        }

        public static T GetAttribute<T>(this MethodInfo methodInfo) where T : Attribute
        {
            return (T)methodInfo.GetCustomAttribute(typeof(T));
        }

        public static bool TryGetAttribute<T>(this MethodInfo methodInfo, out T t) where T : Attribute
        {
            t = methodInfo.GetAttribute<T>();
            return t != null;
        }

        public static T GetAttribute<T>(this ParameterInfo parameterInfo) where T : Attribute
        {
            return (T)parameterInfo.GetCustomAttribute(typeof(T));
        }

        public static bool TryGetAttribute<T>(this ParameterInfo parameterInfo, out T t) where T : Attribute
        {
            t = parameterInfo.GetAttribute<T>();
            return t != null;
        }

        internal static bool HasSpeckDependencies(this Type speckType)
        {
            var dependantPropertyTypes = speckType
                                        .GetProperties()
                                        .Where(propertyInfo => propertyInfo.GetCustomAttribute(typeof(SpeckAutoAttribute)) != null)
                                        .Select(propertyInfo => propertyInfo.PropertyType);

            var dependantFieldTypes = speckType
                                     .GetFields()
                                     .Where(fieldInfo => fieldInfo.GetCustomAttribute(typeof(SpeckAutoAttribute)) != null)
                                     .Select(fieldInfo => fieldInfo.FieldType);

            var dependantContructorParameterTypes = from constructor in speckType.GetConstructors(Constants.BindingFlags)
                                                    from parameterInfo in constructor.GetParameters()
                                                    let SpeckAutoAttribute = parameterInfo.GetAttribute<SpeckAutoAttribute>()
                                                    where SpeckAutoAttribute != null
                                                    select SpeckAutoAttribute.OfType;

            var hasDependendantTypes = dependantPropertyTypes
                                      .Concat(dependantFieldTypes)
                                      .Concat(dependantContructorParameterTypes)
                                      .Distinct()
                                      .Any();

            return hasDependendantTypes;
        }

        internal static List<Type> DependantSpecks(this IEnumerable<Type> speckTypes)
        {
            ThrowForSelfNestedDependencies(speckTypes);

            var dependantPropertyTypes = speckTypes
                                        .SelectMany(type => type.GetProperties())
                                        .Where(propertyInfo => propertyInfo.GetCustomAttribute(typeof(SpeckAutoAttribute)) != null)
                                        .Select(propertyInfo => propertyInfo.PropertyType);

            var dependantFieldTypes = speckTypes
                                     .SelectMany(type => type.GetFields())
                                     .Where(fieldInfo => fieldInfo.GetCustomAttribute(typeof(SpeckAutoAttribute)) != null)
                                     .Select(fieldInfo => fieldInfo.FieldType);

            var dependantContructorParameterTypes = from speckType in speckTypes
                                                    let constructor = speckType.GetConstructors(Constants.BindingFlags).FirstOrDefault()
                                                    where constructor != null
                                                    from parameterInfo in constructor.GetParameters()
                                                    let SpeckAutoAttribute = parameterInfo.GetAttribute<SpeckAutoAttribute>()
                                                    let speckAttribute = parameterInfo.ParameterType.GetAttribute<SpeckAttribute>()
                                                    where speckAttribute != null
                                                    select SpeckAutoAttribute?.OfType ?? parameterInfo.ParameterType;

            return dependantPropertyTypes
                  .Concat(dependantFieldTypes)
                  .Concat(dependantContructorParameterTypes)
                  .Distinct()
                  .ToList();
        }

        internal static List<Type> GetDependencyOrderedSpecks(this IEnumerable<Type> speckTypes)
        {
            var orderedDependencies = new List<Type>();
            var dependantSpecks = speckTypes.DependantSpecks();
            var hasInnerDependencies = dependantSpecks.Any();

            if (hasInnerDependencies)
            {
                var innerDependencies = GetDependencyOrderedSpecks(dependantSpecks);
                orderedDependencies.AddRange(innerDependencies);
            }

            orderedDependencies.AddRange(speckTypes.Except(orderedDependencies));
            return orderedDependencies;
        }

        internal static List<(PropertyInfo PropertyInfo, SpeckAutoAttribute SpeckAutoAttribute)> GetSpeckAutoPropertyAttributeTuples(this Type speckType)
        {
            var tuplePropertySpeckAutos = from propertyInfo in speckType.GetProperties(Constants.BindingFlags)
                                          let SpeckAutoAttribute = propertyInfo.GetCustomAttribute<SpeckAutoAttribute>()
                                          where SpeckAutoAttribute != null
                                          select (propertyInfo, SpeckAutoAttribute);

            return tuplePropertySpeckAutos.ToList();
        }

        internal static List<(FieldInfo FieldInfo, SpeckAutoAttribute SpeckAutoAttribute)> GetSpeckAutoFieldAttributeTuples(this Type speckType)
        {
            var tuples = from fieldInfo in speckType.GetFields(Constants.BindingFlags)
                         let SpeckAutoAttribute = fieldInfo.GetCustomAttribute<SpeckAutoAttribute>()
                         where SpeckAutoAttribute != null
                         select (fieldInfo, SpeckAutoAttribute);

            return tuples.ToList();
        }

        internal static List<ParameterInfo> GetSpeckAutoParameters(this Type speckType)
        {
            return speckType.GetMethods().SelectMany(method => method.GetParameters()).Where(param => param.GetCustomAttribute(typeof(SpeckAutoAttribute)) != null).ToList();
        }

        internal static IEnumerable<(SpeckPostAttribute postSpeckAttribute, MethodInfo methodInfo)> GetSpeckPosts(this Type speckType)
        {
            var results = new List<(SpeckPostAttribute postSpeck, MethodInfo method)>(1);
            foreach (var methodInfo in speckType.GetMethods(Constants.BindingFlags))
            {
                var postSpeck = methodInfo.GetCustomAttribute<SpeckPostAttribute>();
                if (postSpeck != null) results.Add((postSpeck, methodInfo));
            }
            return results.OrderBy(result => result.postSpeck.Order);
        }

        private static void ThrowForSelfNestedDependencies(IEnumerable<Type> speckTypes)
        {
            var nestedSpecks = speckTypes.Where(speckType
                            => speckType.GetProperties().Where(prop => prop.GetCustomAttribute<SpeckAutoAttribute>() != null).Any(prop => prop.PropertyType == speckType)
                            || speckType.GetFields().Where(field => field.GetCustomAttribute<SpeckAutoAttribute>() != null).Any(field => field.FieldType == speckType)
                            || speckType.GetMethods().SelectMany(method => method.GetParameters().Where(param => param.GetCustomAttribute<SpeckAutoAttribute>() != null)).Any(param => param.ParameterType == speckType));

            if (nestedSpecks.Any())
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("You cannot nest a Speck of the same type within itself.");
                stringBuilder.AppendLine("The following nested Specks were found:");

                foreach (var type in nestedSpecks) stringBuilder.AppendLine($"{type.Name}");

                throw new Exception(stringBuilder.ToString());
            }
        }
    }
}
