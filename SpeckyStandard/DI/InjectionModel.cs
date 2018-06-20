using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.DI
{
    internal class InjectionModel
    {
        internal InjectionModel(Type type, object instantiatedObject)
        {
            Type = type;
            ReferencedType = type;
            InjectionMode = InjectionMode.Singleton;
            Instance = instantiatedObject;
        }

        internal InjectionModel(Type type, InjectionMode injectionMode, params object[] parameters)
        {
            Type = type;
            ReferencedType = type;
            InjectionMode = injectionMode;
            Instance = InjectionMode == InjectionMode.Singleton
                     ? Activator.CreateInstance(Type, parameters)
                     : null;
        }

        internal InjectionModel(Type type, Type referencedType, params object[] parameters)
        {
            Type = type;
            ReferencedType = referencedType;
            InjectionMode = InjectionMode.Singleton;
            Instance = InjectionMode == InjectionMode.Singleton
                     ? Activator.CreateInstance(Type, parameters)
                     : null;
        }

        internal Type Type { get; }
        internal object Instance { get; }
        internal Type ReferencedType { get; }
        internal InjectionMode InjectionMode { get; }
    }
}
