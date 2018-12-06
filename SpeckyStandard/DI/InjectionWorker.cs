using SpeckyStandard.Attributes;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using SpeckyStandard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using static SpeckyStandard.Logging.Log;

namespace SpeckyStandard.DI
{
    internal sealed class InjectionWorker
    {
        private readonly Assembly CallingAssembly;
        internal InjectionWorker(Assembly callingAssembly) => CallingAssembly = callingAssembly;      

        internal void Start()
        {
            Print("Locating Configurations.", PrintType.DebugWindow);

            var speckyConfigurations = CallingAssembly
                                      .TypesWithAttribute<SpeckConfigurationAttribute>(speckConfiguration => speckConfiguration.Configuration == GlobalConfiguration.Profile);

            Print("Injecting Configurations.", PrintType.DebugWindow);

            foreach (var speckConfiguration in speckyConfigurations)
                InjectNewConfiguration(speckConfiguration);

            Print("Locating Specks.", PrintType.DebugWindow);

            var specks = CallingAssembly
                        .TypesWithAttribute<SpeckAttribute>();

            Print("Injecting Specks.", PrintType.DebugWindow);

            foreach (var speck in specks)
                SpeckContainer
                    .Instance
                    .HasSpeck(speck)
                    .Pulse(False: () => InjectNewSpeck(speck));
        }

        private static void InjectNewConfiguration(Type configurationType)
        {
            SpeckContainer
                .Instance
                .InjectConfiguration(configurationType);

            configurationType
                .TryGetAttribute(out SpeckLogAttribute speckLog)
                .Pulse(True: () => Print(speckLog.Message, speckLog.PrintType));
        }

        private static object InjectNewSpeck(Type speckType)
        {
            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            var initializingObject = FormatterServices.GetUninitializedObject(speckType);

            switch (speckAttribute.SpeckType)
            {
                case SpeckType.PerRequest:
                    initializingObject = SpeckContainer
                        .Instance
                        .InjectType(speckType)
                        .Instance;
                    break;

                case SpeckType.Singleton:
                default:
                    InjectNewSpeckProperties(speckType, initializingObject);
                    InjectNewSpeckFields(speckType, initializingObject);
                    SetSpeckConfigurationProperties(speckType, initializingObject);
                    SetSpeckConfigurationFields(speckType, initializingObject);
                    InvokeDefaultSpeckContructor(speckType, initializingObject);

                    SpeckContainer
                        .Instance
                        .InjectSingleton(initializingObject, speckAttribute?.ReferencedType);
                    break;

            }

            return initializingObject;
        }

        private static object InvokeDefaultSpeckContructor(Type speckType, object unitializedObject)
        {
            var unitializedObjectConstructor = unitializedObject
                                              .GetType()
                                              .GetConstructors(Constants.BindingFlags)
                                              .FirstOrDefault();

            var initializedParameters = new List<object>();

            foreach (var parameterInfo in unitializedObjectConstructor.GetParameters())
            {
                if (parameterInfo.TryGetAttribute(out SpeckConfigurationAutoAttribute speckConfiguration))
                {
                    var key = speckConfiguration.Key;
                    if (string.IsNullOrEmpty(key)) key = parameterInfo.Name;

                    initializedParameters.Add(SpeckContainer.Instance.GetConfigurationValue(key));
                    continue;
                }

                var speckInstance = GetOrInjectSpeck(parameterInfo.GetAttribute<SpeckAutoAttribute>()?.OfType ?? parameterInfo.ParameterType);

                if (speckInstance != null)
                {
                    initializedParameters.Add(speckInstance);
                    continue;
                }

                throw new ArgumentNullException($"Contructor of {nameof(speckType.Name)} has parameter with missing speck.");
            }

            unitializedObjectConstructor.Invoke(unitializedObject, initializedParameters.Any() ? initializedParameters.ToArray() : null);
            return unitializedObject;
        }

        private static void InjectNewSpeckProperties(Type speckType, object unitializedObject)
        {
            var tuples = speckType.GetPropertyAttributeTuples<SpeckAutoAttribute>();

            foreach (var tuple in tuples)
            {
                var instanceType = tuple.Attribute.OfType
                                ?? tuple.PropertyInfo.PropertyType;

                var speckInstance = GetOrInjectSpeck(instanceType);

                SetPropertyValue(unitializedObject, tuple.PropertyInfo, speckInstance);

                tuple
                    .PropertyInfo
                    .TryGetAttribute(out SpeckLogAttribute speckLog)
                    .Pulse(True: () => Print($"{speckType.Name} auto initialized. {speckLog.Message}", speckLog.PrintType));
            }
        }

        private static void InjectNewSpeckFields(Type speckType, object unitializedObject)
        {
            var tuples = speckType.GetFieldAttributeTuples<SpeckAutoAttribute>();

            foreach (var tuple in tuples)
            {
                var instanceType = tuple.Attribute.OfType
                                ?? tuple.FieldInfo.FieldType;

                var speckInstance = GetOrInjectSpeck(instanceType);

                tuple
                    .FieldInfo
                    .SetValue(unitializedObject, SpeckContainer.Instance.GetInstance(instanceType), Constants.BindingFlags, null, null);
                tuple
                    .FieldInfo
                    .TryGetAttribute(out SpeckLogAttribute speckLog)
                    .Pulse(True: () => Print($"{speckType.Name} auto initialized. {speckLog.Message}", speckLog.PrintType));
            }
        }

        private static void SetSpeckConfigurationProperties(Type speckType, object unitializedObject)
        {
            var tuples = speckType.GetPropertyAttributeTuples<SpeckConfigurationAutoAttribute>();
            foreach (var tuple in tuples)
            {
                var key = tuple.Attribute.Key;
                if (string.IsNullOrEmpty(key)) key = tuple.PropertyInfo.Name;

                var configurationValue = SpeckContainer.Instance.GetConfigurationValue(key);

                SetPropertyValue(unitializedObject, tuple.PropertyInfo, configurationValue);

                tuple
                    .PropertyInfo
                    .TryGetAttribute(out SpeckLogAttribute speckLog)
                    .Pulse(True: () => Print($"{speckType.Name} auto initialized. {speckLog.Message}", speckLog.PrintType));
            }
        }

        private static void SetSpeckConfigurationFields(Type speckType, object unitializedObject)
        {
            var tuples = speckType.GetFieldAttributeTuples<SpeckConfigurationAutoAttribute>();

            foreach (var tuple in tuples)
            {
                var key = tuple.Attribute.Key;
                if (string.IsNullOrEmpty(key)) key = tuple.FieldInfo.Name;

                var configurationValue = SpeckContainer.Instance.GetConfigurationValue(key);

                tuple
                    .FieldInfo
                    .SetValue(unitializedObject, configurationValue, Constants.BindingFlags, null, null);
                tuple
                    .FieldInfo
                    .TryGetAttribute(out SpeckLogAttribute speckLog)
                    .Pulse(True: () => Print($"{speckType.Name} auto initialized. {speckLog.Message}", speckLog.PrintType));
            }
        }

        private static object GetOrInjectSpeck(Type instanceType)
        {
            var speckInstance = SpeckContainer.Instance.GetInstance(instanceType, throwable: false);
            if (speckInstance == null) speckInstance = InjectNewSpeck(instanceType) ?? throw new NullReferenceException($"Trying to inject null {instanceType.Name} into a Speck");
            return speckInstance;
        }

        private static void SetPropertyValue(object formattedObject, PropertyInfo speckProperty, object speckInstance)
        {
            if (speckProperty.CanWrite)
            {
                speckProperty.SetValue(formattedObject, speckInstance);
            }
            else
            {
                var backField = speckProperty.ReflectedType
                               .GetFields(Constants.BindingFlags)
                               .Where(fieldInfo =>
                               {
                                   return fieldInfo.Name.StartsWith($"<{speckProperty.Name}>")
                                       && fieldInfo.Name.EndsWith("BackingField");
                               }).FirstOrDefault();

                if (backField != null)
                {
                    backField.SetValue(formattedObject, speckInstance);
                }
                else
                {
                    Print($"Cannot set SpeckAuto readonly property {nameof(speckProperty.Name)}.", DebugSettings.DebugPrintType);
                }
            }
        }
    }
}
