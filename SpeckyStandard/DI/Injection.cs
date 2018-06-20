using SpeckyStandard.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeckyStandard.DI
{
    public class Injection
    {
        internal HashSet<InjectionModel> Singletons { get; } = new HashSet<InjectionModel>();

        Injection() { }
        static public Injection Instance { get; } = new Injection();

        public T GetInstance<T>() => (T)GetInstance(typeof(T));

        internal void InjectSingleton(Type type, Type referencedType)
        {
            var injectionModel = new InjectionModel(
                                     type: type, 
                                     referencedType: referencedType);

            Singletons.Add(injectionModel);
        }

        internal void InjectSingleton(object formattedObject, Type referencedType)
        {
            var type = formattedObject.GetType();
            var injectionModel = new InjectionModel(type, formattedObject);
            Singletons.Add(injectionModel);
        }

        internal void InjectType<T>() => InjectType(typeof(T));

        internal void InjectType(Type type)
        {
            var injectionModel = new InjectionModel(
                                     type: type,
                                     injectionMode: InjectionMode.PerRequest);

            Singletons.Add(injectionModel);
        }

        internal object GetInstance(Type type, bool throwable = true)
        {
            var injectionModel = Singletons.FirstOrDefault(model => model.Type == type || model.ReferencedType == type);

            switch (injectionModel?.InjectionMode)
            {
                case InjectionMode.PerRequest:
                    var newSpeck = Activator.CreateInstance(injectionModel.Type);
                    return newSpeck;
                case InjectionMode.Singleton:
                    return injectionModel.Instance;
                default :
                    if (throwable)
                    {
                        throw new Exception($"Type: {type.Name} not injected");
                    }
                    else
                    {
                        return null;
                    }
            }
        }
    }
}
