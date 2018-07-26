using SpeckyStandard.Attributes;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using SpeckyStandard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SpeckyStandard.DI
{
    internal sealed class InjectionWorker
    {
        private readonly Assembly CallindAssembly;
        internal InjectionWorker(Assembly callingAssembly) => CallindAssembly = callingAssembly;

        internal void Start()
        {
            Log.Print("Locating Specks.", PrintType.DebugWindow);

            var speckTypes = CallindAssembly.TypesWithAttribute<SpeckAttribute>();
            speckTypes = speckTypes.ToList();

            Log.Print("Ordering Specks.", PrintType.DebugWindow);

            speckTypes = speckTypes.GetDependencyOrderedSpecks();

            Log.Print("Injecting Specks.", PrintType.DebugWindow);

            InjectOrderedSpecks(speckTypes);
        }

        private void InjectOrderedSpecks(IEnumerable<Type> speckTypes)
        {
            var formattersStillAwaitingConstruction = new List<object>();
            foreach (var speckType in speckTypes)
            {
                if (speckType.HasSpeckDependencies())
                {
                    InjectPartialSpeck(formattersStillAwaitingConstruction, speckType);
                }
                else
                {
                    InjectFullSpeck(speckType);
                }
            }
        }

        private void InjectFullSpeck(Type speckType)
        {
            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            var injectionMode = speckAttribute?.SpeckType ?? SpeckType.Singleton;

            switch (injectionMode)
            {
                case SpeckType.Singleton:
                    if (speckType.IsInterface) break;
                    SpeckContainer.Instance.InjectSingleton(speckType, speckAttribute?.ReferencedType);
                    break;
                case SpeckType.PerRequest:
                    SpeckContainer.Instance.InjectType(speckType);
                    break;
                default:
                    throw new Exception($"Unknown {nameof(SpeckType)}");
            }
        }

        private void InjectPartialSpeck(List<object> formattersStillAwaitingConstruction, Type speckType)
        {
            var formattedObject = FormatterServices.GetUninitializedObject(speckType);

            SetAutoSpeckProperties(speckType, formattedObject);
            SetAutoSpeckFields(speckType, formattedObject);

            formattersStillAwaitingConstruction.Add(formattedObject);

            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            var injectionMode = speckAttribute?.SpeckType ?? SpeckType.Singleton;

            if (injectionMode != SpeckType.Singleton)
            {
                throw new Exception($"Specks containing auto specks can only use default {nameof(SpeckType)}.{nameof(SpeckType.Singleton)}\n{formattedObject.GetType().Name} is set as {nameof(SpeckType)}.{injectionMode.ToString()}");
            }

            formattedObject.GetType().GetConstructor(Type.EmptyTypes).Invoke(formattedObject, null);
            SpeckContainer.Instance.InjectSingleton(formattedObject, speckAttribute?.ReferencedType);
        }

        private static void SetAutoSpeckProperties(Type speckType, object formattedObject)
        {
            var speckProperties = speckType.GetAutoSpeckProperties();
            foreach (var speckProperty in speckProperties)
                SetPropertyValue(formattedObject, speckProperty);
        }

        private static void SetAutoSpeckFields(Type speckType, object formattedObject)
        {
            var speckFields = speckType.GetAutoSpeckFields();
            foreach (var speckField in speckFields)
                speckField.SetValue(formattedObject, SpeckContainer.Instance.GetInstance(speckField.FieldType), Constants.BindingFlags, null, null);
        }

        private static void SetPropertyValue(object formattedObject, PropertyInfo speckProperty)
        {
            if (speckProperty.CanWrite)
            {
                speckProperty.SetValue(formattedObject, SpeckContainer.Instance.GetInstance(speckProperty.PropertyType));
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
                    backField.SetValue(formattedObject, SpeckContainer.Instance.GetInstance(speckProperty.PropertyType));
                }
                else
                {
                    Log.Print($"Cannot set AutoSpeck readonly property {nameof(speckProperty.Name)}.", DebugSettings.DebugPrintType);
                }
            }
        }
    }
}
