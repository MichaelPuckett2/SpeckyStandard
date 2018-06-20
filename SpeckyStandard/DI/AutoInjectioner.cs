using SpeckyStandard.Attributes;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SpeckyStandard.DI
{
    internal class AutoInjectioner
    {
        private readonly Assembly CallindAssembly;
        internal AutoInjectioner(Assembly callingAssembly) => CallindAssembly = callingAssembly;

        internal void Start()
        {
            var speckTypes = CallindAssembly.TypesWithAttribute<SpeckAttribute>().ToList();
            speckTypes = speckTypes.GetDependencyOrderedSpecks();
            InjectOrderedSpecks(speckTypes);
        }

        private static void InjectOrderedSpecks(IEnumerable<Type> speckTypes)
        {
            var formattersStillAwaitingConstruction = new List<object>();
            foreach (var speckType in speckTypes)
            {
                var knownSpeck = Injection.Instance.GetInstance(speckType, false)?.GetType();

                var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
                var injectionMode = speckAttribute?.InjectionMode ?? InjectionMode.Singleton;

                if (speckType.HasSpeckDependencies())
                {
                    InjectPartialSpeck(formattersStillAwaitingConstruction, speckType, injectionMode);
                }
                else
                {
                    InjectFullSpeck(speckType, injectionMode);
                }
            }
        }

        private static void InjectFullSpeck(Type speckType, InjectionMode injectionMode)
        {
            switch (injectionMode)
            {
                case InjectionMode.Singleton:
                    if (speckType.IsInterface) break;
                    var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
                    Injection.Instance.InjectSingleton(speckType, speckAttribute?.ReferencedType);
                    break;
                case InjectionMode.PerRequest:
                    Injection.Instance.InjectType(speckType);
                    break;
                default:
                    throw new Exception($"Unknown {nameof(InjectionMode)}");
            }
        }

        private static void InjectPartialSpeck(List<object> formattersStillAwaitingConstruction, Type speckType, InjectionMode injectionMode)
        {
            var formattedObject = FormatterServices.GetUninitializedObject(speckType);

            var speckProperties = speckType.GetAutoSpeckProperties();

            foreach (var speckProperty in speckProperties)
            {
                if (!speckProperty.CanWrite) throw new Exception($"Readonly properties cannot be instialized via {nameof(AutoSpeckAttribute)}.  Try giving the property a private setter.\nThrow on {nameof(speckProperty.Name)}");
                speckProperty.SetValue(formattedObject, Injection.Instance.GetInstance(speckProperty.PropertyType));
            }

            var speckFields = speckType.GetAutoSpeckFields();

            foreach (var speckField in speckFields)
            {
                speckField.SetValue(formattedObject, Injection.Instance.GetInstance(speckField.FieldType));
            }

            formattersStillAwaitingConstruction.Add(formattedObject);

            if (injectionMode != InjectionMode.Singleton)
            {
                throw new Exception($"Specks containing auto specks can only use default {nameof(InjectionMode)}.{nameof(InjectionMode.Singleton)}\n{formattedObject.GetType().Name} is set as {nameof(InjectionMode)}.{injectionMode.ToString()}");
            }

            formattedObject.GetType().GetConstructor(Type.EmptyTypes).Invoke(formattedObject, null);

            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            Injection.Instance.InjectSingleton(formattedObject, speckAttribute?.ReferencedType);
        }
    }
}
