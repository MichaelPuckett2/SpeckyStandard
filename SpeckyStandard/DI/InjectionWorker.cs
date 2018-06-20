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
    internal class InjectionWorker
    {
        private readonly Assembly CallindAssembly;
        internal InjectionWorker(Assembly callingAssembly) => CallindAssembly = callingAssembly;

        internal void Start()
        {
            var speckTypes = CallindAssembly.TypesWithAttribute<SpeckAttribute>().ToList();
            speckTypes = speckTypes.GetDependencyOrderedSpecks();
            InjectOrderedSpecks(speckTypes);
        }

        private void InjectOrderedSpecks(IEnumerable<Type> speckTypes)
        {
            var formattersStillAwaitingConstruction = new List<object>();
            foreach (var speckType in speckTypes)
            {
                var knownSpeck = SpeckContainer.Instance.GetInstance(speckType, false)?.GetType();

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

        private void InjectFullSpeck(Type speckType, InjectionMode injectionMode)
        {
            switch (injectionMode)
            {
                case InjectionMode.Singleton:
                    if (speckType.IsInterface) break;
                    var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
                    SpeckContainer.Instance.InjectSingleton(speckType, speckAttribute?.ReferencedType);
                    break;
                case InjectionMode.PerRequest:
                    SpeckContainer.Instance.InjectType(speckType);
                    break;
                default:
                    throw new Exception($"Unknown {nameof(InjectionMode)}");
            }
        }

        private void InjectPartialSpeck(List<object> formattersStillAwaitingConstruction, Type speckType, InjectionMode injectionMode)
        {
            var formattedObject = FormatterServices.GetUninitializedObject(speckType);

            var speckProperties = speckType.GetAutoSpeckProperties();

            foreach (var speckProperty in speckProperties)
            {
                if (!speckProperty.CanWrite) throw new Exception($"Readonly properties cannot be instialized via {nameof(AutoSpeckAttribute)}.  Try giving the property a private setter.\nThrow on {nameof(speckProperty.Name)}");
                speckProperty.SetValue(formattedObject, SpeckContainer.Instance.GetInstance(speckProperty.PropertyType));
            }

            var speckFields = speckType.GetAutoSpeckFields();

            foreach (var speckField in speckFields)
            {
                speckField.SetValue(formattedObject, SpeckContainer.Instance.GetInstance(speckField.FieldType));
            }

            formattersStillAwaitingConstruction.Add(formattedObject);

            if (injectionMode != InjectionMode.Singleton)
            {
                throw new Exception($"Specks containing auto specks can only use default {nameof(InjectionMode)}.{nameof(InjectionMode.Singleton)}\n{formattedObject.GetType().Name} is set as {nameof(InjectionMode)}.{injectionMode.ToString()}");
            }

            formattedObject.GetType().GetConstructor(Type.EmptyTypes).Invoke(formattedObject, null);

            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            SpeckContainer.Instance.InjectSingleton(formattedObject, speckAttribute?.ReferencedType);
        }
    }
}
