using SpeckyStandard.Attributes;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using SpeckyStandard.Logging;
using System;
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
            Print("Locating Specks.", PrintType.DebugWindow);

            StartConfigs();
            StartSpecks();
        }

        private void StartConfigs()
        {
            var configTypes = CallingAssembly
                             .TypesWithAttribute<SpeckConfigurationAttribute>(attribute => attribute.Profile == GlobalConfiguration.Profile)
                             .WithEach((Type configType) =>
                             {
                                 SpeckContainer
                                 .Instance
                                 .InjectSingleton(configType)
                                 .TryGetAttribute(out SpeckLogAttribute speckLog)
                                 .PulseOnTrue(() => Print(speckLog.Message, speckLog.PrintType));
                             });
        }

        private void StartSpecks()
        {
            CallingAssembly
               .Log($"Specky Started {DateTime.Now}", PrintType.DebugWindow)
               .TypesWithAttribute<SpeckAttribute>()
               .Log("Ordering Specks.", PrintType.DebugWindow)
               .GetDependencyOrderedSpecks()
               .Log("Injecting Specks.", PrintType.DebugWindow)
               .WithEach((Type speckType) =>
               {
                   speckType
                       .HasSpeckDependencies()
                       .Pulse(() => InjectPartialSpeck(speckType),
                              () => InjectFullSpeck(speckType));
               })
               .SelectMany(speck => speck.GetSpeckPosts())
               .WithEach(((SpeckPostAttribute speckPost, MethodInfo method) tuple) =>
               {
                   Print($"SpeckPost {tuple.method.DeclaringType.Name}:{tuple.method.Name}", PrintType.DebugWindow);

                   tuple
                    .method
                    .Invoke(SpeckContainer.Instance.GetInstance(tuple.method.DeclaringType), null);

                   tuple
                    .method
                    .TryGetAttribute(out SpeckLogAttribute speckLog)
                    .PulseOnTrue(() => Print(speckLog.Message, speckLog.PrintType));
               });
        }

        private void InjectFullSpeck(Type speckType)
        {
            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            var injectionMode = speckAttribute?.SpeckType ?? SpeckType.Singleton;

            switch (injectionMode)
            {
                case SpeckType.Singleton:
                    if (speckType.IsInterface) break;

                    var parameterTypes = speckType
                                        .GetConstructors(Constants.BindingFlags)
                                        .FirstOrDefault()?
                                        .GetParameters()
                                        .ToArray();

                    var instances = SpeckContainer.Instance.GetInstances(parameterTypes);
                    SpeckContainer.Instance.InjectSingleton(speckType, speckAttribute?.ReferencedType, instances);
                    break;
                case SpeckType.PerRequest:
                    SpeckContainer.Instance.InjectType(speckType);
                    break;
                default:
                    throw new Exception($"Unknown {nameof(SpeckType)}");
            }

            Print($"Injected {speckType.Name}", PrintType.DebugWindow);
        }

        private void InjectPartialSpeck(Type speckType)
        {
            var formattedObject = FormatterServices.GetUninitializedObject(speckType);

            SetSpeckAutoProperties(speckType, formattedObject);
            SetSpeckAutoFields(speckType, formattedObject);

            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            var injectionMode = speckAttribute?.SpeckType ?? SpeckType.Singleton;

            if (injectionMode != SpeckType.Singleton)
            {
                throw new Exception($"Specks containing auto specks can only use default {nameof(SpeckType)}.{nameof(SpeckType.Singleton)}\n{formattedObject.GetType().Name} is set as {nameof(SpeckType)}.{injectionMode.ToString()}");
            }

            var formattedObjectContructor = formattedObject
                                           .GetType()
                                           .GetConstructors(Constants.BindingFlags)
                                           .FirstOrDefault();

            var parameterTypes = formattedObjectContructor?
                                .GetParameters()
                                .Select(parameterInfo =>
                                {
                                    var SpeckAutoAttribute = parameterInfo.GetAttribute<SpeckAutoAttribute>();
                                    return SpeckAutoAttribute?.OfType ?? parameterInfo.ParameterType;
                                })
                                .ToArray();

            if (!parameterTypes?.Any() ?? false)
            {
                formattedObject
                    .GetType()
                    .GetConstructor(Type.EmptyTypes)
                    .Invoke(formattedObject, null);
            }
            else
            {
                try
                {
                    var instances = SpeckContainer
                        .Instance
                        .GetInstances(parameterTypes);

                    var constructor = formattedObject
                        .GetType()
                        .GetConstructor(parameterTypes);

                    constructor.Invoke(formattedObject, instances);
                }
                catch (TargetParameterCountException)
                {
                    Print($"{speckType.Name} has a constructor looking for types"
                          + $" {parameterTypes.Select(parameterInfo => parameterInfo.Name).ToDelimitedString(", ")}"
                          + " however if appears they aren't Specked.\n"
                          + $"Try adding {nameof(SpeckAutoAttribute)} to the parameter and specifying a typeof if the parameter is the base of another Speck.\n"
                          + "Example: If your parameter expected a specific type but you have a derived type Specked: \n"
                          + "[SpeckAuto(typeof(TestViewModel))] INotifyProperyChanged viewModel",
                          PrintType.DebugWindow | PrintType.ThrowException);
                }
            }



            SpeckContainer
                .Instance
                .InjectSingleton(formattedObject, speckAttribute?.ReferencedType);

            Print($"Injected {speckType.Name}", PrintType.DebugWindow);
        }

        private static void SetSpeckAutoProperties(Type speckType, object formattedObject)
        {
            var tuples = speckType.GetSpeckAutoPropertyAttributeTuples();

            foreach (var tuple in tuples)
            {
                var instanceType = tuple.SpeckAutoAttribute.OfType
                                ?? tuple.PropertyInfo.PropertyType;

                SetPropertyValue(formattedObject, tuple.PropertyInfo, instanceType);
                tuple
                    .PropertyInfo
                    .TryGetAttribute(out SpeckLogAttribute speckLog)
                    .PulseOnTrue(() => Print($"{speckType.Name} auto initialized. {speckLog.Message}", speckLog.PrintType));
            }
        }

        private static void SetPropertyValue(object formattedObject, PropertyInfo speckProperty, Type instanceType)
        {
            if (speckProperty.CanWrite)
            {
                speckProperty.SetValue(formattedObject, SpeckContainer.Instance.GetInstance(instanceType));
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
                    backField.SetValue(formattedObject, SpeckContainer.Instance.GetInstance(instanceType));
                }
                else
                {
                    Print($"Cannot set SpeckAuto readonly property {nameof(speckProperty.Name)}.", DebugSettings.DebugPrintType);
                }
            }
        }

        private static void SetSpeckAutoFields(Type speckType, object formattedObject)
        {
            var tuples = speckType.GetSpeckAutoFieldAttributeTuples();

            foreach (var tuple in tuples)
            {
                var instanceType = tuple.SpeckAutoAttribute.OfType
                                ?? tuple.FieldInfo.FieldType;

                tuple
                    .FieldInfo
                    .SetValue(formattedObject, SpeckContainer.Instance.GetInstance(instanceType), Constants.BindingFlags, null, null);
                tuple
                    .FieldInfo
                    .TryGetAttribute(out SpeckLogAttribute speckLog)
                    .PulseOnTrue(() => Print($"{speckType.Name} auto initialized. {speckLog.Message}", speckLog.PrintType));
            }
        }
    }
}
