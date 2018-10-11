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

        public static T GetAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return (T)propertyInfo.GetCustomAttribute(typeof(T));
        }

        public static T GetAttribute<T>(this FieldInfo fieldInfo) where T : Attribute
        {
            return (T)fieldInfo.GetCustomAttribute(typeof(T));
        }

        public static T GetAttribute<T>(this ParameterInfo parameterInfo) where T : Attribute
        {
            return (T)parameterInfo.GetCustomAttribute(typeof(T));
        }

        internal static bool HasSpeckDependencies(this Type speckType)
        {
            var dependantPropertyTypes = speckType
                                        .GetProperties()
                                        .Where(propertyInfo => propertyInfo.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                        .Select(propertyInfo => propertyInfo.PropertyType);

            var dependantFieldTypes = speckType
                                     .GetFields()
                                     .Where(fieldInfo => fieldInfo.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                     .Select(fieldInfo => fieldInfo.FieldType);

            var dependantContructorParameterTypes = from constructor in speckType.GetConstructors(Constants.BindingFlags)
                                                    from parameterInfo in constructor.GetParameters()
                                                    let autoSpeckAttribute = parameterInfo.GetAttribute<AutoSpeckAttribute>()
                                                    where autoSpeckAttribute != null
                                                    select autoSpeckAttribute.OfType;

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
                                        .Where(propertyInfo => propertyInfo.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                        .Select(propertyInfo => propertyInfo.PropertyType);

            var dependantFieldTypes = speckTypes
                                     .SelectMany(type => type.GetFields())
                                     .Where(fieldInfo => fieldInfo.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                     .Select(fieldInfo => fieldInfo.FieldType);

            var dependantContructorParameterTypes = from speckType in speckTypes
                                                    let constructor = speckType.GetConstructors(Constants.BindingFlags).FirstOrDefault()
                                                    where constructor != null
                                                    from parameterInfo in constructor.GetParameters()
                                                    let autoSpeckAttribute = parameterInfo.GetAttribute<AutoSpeckAttribute>()
                                                    let speckAttribute = parameterInfo.ParameterType.GetAttribute<SpeckAttribute>()
                                                    where speckAttribute != null
                                                    select autoSpeckAttribute?.OfType ?? parameterInfo.ParameterType;

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

        internal static List<(PropertyInfo PropertyInfo, AutoSpeckAttribute AutoSpeckAttribute)> GetAutoSpeckPropertyAttributeTuples(this Type speckType)
        {
            var tuplePropertyAutoSpecks = from propertyInfo in speckType.GetProperties(Constants.BindingFlags)
                                          let autoSpeckAttribute = propertyInfo.GetCustomAttribute<AutoSpeckAttribute>()
                                          where autoSpeckAttribute != null
                                          select (propertyInfo, autoSpeckAttribute);

            return tuplePropertyAutoSpecks.ToList();
        }

        internal static List<(FieldInfo FieldInfo, AutoSpeckAttribute AutoSpeckAttribute)> GetAutoSpeckFieldAttributeTuples(this Type speckType)
        {
            var tuples = from fieldInfo in speckType.GetFields(Constants.BindingFlags)
                         let autoSpeckAttribute = fieldInfo.GetCustomAttribute<AutoSpeckAttribute>()
                         where autoSpeckAttribute != null
                         select (fieldInfo, autoSpeckAttribute);

            return tuples.ToList();
        }

        internal static List<ParameterInfo> GetAutoSpeckParameters(this Type speckType)
        {
            return speckType.GetMethods().SelectMany(method => method.GetParameters()).Where(param => param.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null).ToList();
        }

        private static void ThrowForSelfNestedDependencies(IEnumerable<Type> speckTypes)
        {
            var nestedSpecks = speckTypes.Where(speckType
                            => speckType.GetProperties().Where(prop => prop.GetCustomAttribute<AutoSpeckAttribute>() != null).Any(prop => prop.PropertyType == speckType)
                            || speckType.GetFields().Where(field => field.GetCustomAttribute<AutoSpeckAttribute>() != null).Any(field => field.FieldType == speckType)
                            || speckType.GetMethods().SelectMany(method => method.GetParameters().Where(param => param.GetCustomAttribute<AutoSpeckAttribute>() != null)).Any(param => param.ParameterType == speckType));

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
