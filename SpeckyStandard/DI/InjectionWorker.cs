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
    internal sealed class InjectionWorker
    {
        private readonly Assembly CallindAssembly;
        internal InjectionWorker(Assembly callingAssembly) => CallindAssembly = callingAssembly;

        internal void Start()
        {
            var speckTypes = CallindAssembly.TypesWithAttribute<SpeckAttribute>();
            //speckTypes = speckTypes.Concat(CallindAssembly.TypesWithAttribute<DalBaseAttribute>());

            speckTypes = speckTypes.ToList();
            speckTypes = speckTypes.GetDependencyOrderedSpecks();
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

            var speckAttribute = speckType.GetAttribute<SpeckAttribute>();
            var injectionMode = speckAttribute?.SpeckType ?? SpeckType.Singleton;

            if (injectionMode != SpeckType.Singleton)
            {
                throw new Exception($"Specks containing auto specks can only use default {nameof(SpeckType)}.{nameof(SpeckType.Singleton)}\n{formattedObject.GetType().Name} is set as {nameof(SpeckType)}.{injectionMode.ToString()}");
            }

            formattedObject.GetType().GetConstructor(Type.EmptyTypes).Invoke(formattedObject, null);
            SpeckContainer.Instance.InjectSingleton(formattedObject, speckAttribute?.ReferencedType);
        }
    }
}
