using SpeckyStandard.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpeckyStandard.Extensions
{
    internal static class SpeckExtensions
    {
        internal static BindingFlags BindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return (T)type.GetCustomAttribute(typeof(T));
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

            return dependantPropertyTypes.Concat(dependantFieldTypes).Concat(dependantParameterTypes).Distinct().ToList();
        }

        internal static List<Type> GetDependencyOrderedSpecks(this IEnumerable<Type> speckTypes)
        {
            var orderedDependencies = new List<Type>();
            var dependantTypes = speckTypes.DependantSpecks();
            var hasInnerDependencies = dependantTypes.Any();

            if (hasInnerDependencies)
            {
                //var illegalDependants = dependantTypes.Where(type => type.GetCustomAttribute<SpeckAttribute>() == null);

                //if (illegalDependants.Any())
                //{
                //    var stringBuilder = new StringBuilder();
                //    foreach (var type in illegalDependants) stringBuilder.AppendLine(type.Name);
                //    throw new Exception($"The following types are given the {nameof(AutoSpeckAttribute)} but are not covered by the {nameof(SpeckAttribute)}\n{stringBuilder.ToString()}");
                //}

                var innerDependencies = GetDependencyOrderedSpecks(dependantTypes);
                orderedDependencies.AddRange(innerDependencies);
            }

            orderedDependencies.AddRange(speckTypes.Except(orderedDependencies));
            return orderedDependencies;
        }

        internal static List<PropertyInfo> GetAutoSpeckProperties(this Type speckType)
        {
            return speckType.GetProperties(BindingFlag).Where(prop => prop.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null).ToList();
        }

        internal static List<FieldInfo> GetAutoSpeckFields(this Type speckType)
        {
            return speckType.GetFields(BindingFlag).Where(field => field.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null).ToList();
        }

        internal static List<ParameterInfo> GetAutoSpeckParameters(this Type speckType)
        {
            return speckType.GetMethods().SelectMany(method => method.GetParameters()).Where(param => param.GetCustomAttribute(typeof(AutoSpeckAttribute)) != null).ToList();
        }
    }
}
