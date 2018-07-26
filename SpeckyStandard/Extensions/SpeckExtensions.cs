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
        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return (T)type.GetCustomAttribute(typeof(T));
        }

        public static T GetAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return (T)propertyInfo.GetCustomAttribute(typeof(T));
        }

        internal static bool HasSpeckDependencies(this Type speckType)
        {
            var dependantPropertyTypes = speckType
                                        .GetProperties()
                                        .Where(prop => prop.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                        .Select(prop => prop.PropertyType);

            var dependantFieldTypes = speckType
                                     .GetFields()
                                     .Where(field => field.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                     .Select(field => field.FieldType);

            var dependantParameterTypes = speckType
                                         .GetMethods()
                                         .SelectMany(method => method.GetParameters())
                                         .Where(param => param.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                         .Select(param => param.ParameterType);

            var hasDependendantTypes = dependantPropertyTypes.Concat(dependantFieldTypes).Concat(dependantParameterTypes).Distinct().Any();

            return hasDependendantTypes;
        }

        internal static List<Type> DependantSpecks(this IEnumerable<Type> speckTypes)
        {
            ThrowForNestedDependencies(speckTypes);

            var dependantPropertyTypes = speckTypes
                                        .SelectMany(type => type.GetProperties())
                                        .Where(prop => prop.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                        .Select(prop => prop.PropertyType);

            var dependantFieldTypes = speckTypes
                                     .SelectMany(type => type.GetFields())
                                     .Where(field => field.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                     .Select(field => field.FieldType);

            var dependantParameterTypes = speckTypes
                                         .SelectMany(type => type.GetMethods())
                                         .SelectMany(method => method.GetParameters())
                                         .Where(param => param.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null)
                                         .Select(param => param.ParameterType);

            ThrowIfNotGivenSpeckAttribute(dependantPropertyTypes);
            ThrowIfNotGivenSpeckAttribute(dependantFieldTypes);
            ThrowIfNotGivenSpeckAttribute(dependantParameterTypes);

            return dependantPropertyTypes.Concat(dependantFieldTypes).Concat(dependantParameterTypes).Distinct().ToList();
        }

        private static void ThrowIfNotGivenSpeckAttribute(IEnumerable<Type> dependantPropertyTypes)
        {
            foreach (var type in dependantPropertyTypes)
                ThrowIfNotGivenSpeckAttribute(type);
        }

        private static void ThrowIfNotGivenSpeckAttribute(Type type)
        {
            if (!type.IsInterface && type.GetAttribute<SpeckAttribute>() == null)
                throw new Exception($"{type.Name} is a dependant of another Speck but does not have it's own {nameof(SpeckAttribute)} provided.\nTypes given the {nameof(AutoSpeckAttribute)}, as a dependant of another Speck, must have the {nameof(SpeckAttribute)}");
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

        private static void ThrowForNestedDependencies(IEnumerable<Type> speckTypes)
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
