using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using System;

namespace SpeckyStandard.DI
{
    internal sealed class InjectionModel
    {
        internal InjectionModel(Type type, object instantiatedObject)
        {
            Type = type;
            ReferencedType = type;
            InjectionMode = SpeckType.Singleton;
            Instance = instantiatedObject;
        }

        internal InjectionModel(Type type, SpeckType injectionMode, params object[] parameters)
        {
            Type = type;
            ReferencedType = type;
            InjectionMode = injectionMode;
            Instance = InjectionMode == SpeckType.Singleton
                     ? Activator.CreateInstance(Type, parameters)
                     : null;
        }

        internal InjectionModel(Type type, Type referencedType, params object[] parameters)
        {
            Type = type;
            ReferencedType = referencedType;
            InjectionMode = SpeckType.Singleton;
            Instance = InjectionMode == SpeckType.Singleton
                     ? Activator.CreateInstance(Type, parameters)
                     : null;
        }

        internal Type Type { get; }
        internal object Instance { get; }
        internal Type ReferencedType { get; }
        internal SpeckType InjectionMode { get; }
        internal T GetAttribute<T>() where T : Attribute => Type.GetAttribute<T>() ?? ReferencedType.GetAttribute<T>();
    }
}
