using SpeckyStandard.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeckyStandard.DI
{
    /// <summary>
    /// The Speck container that is used to store and reference existing specks.
    /// </summary>
    public sealed class SpeckContainer
    {
        internal HashSet<InjectionModel> InjectionModels { get; } = new HashSet<InjectionModel>();

        SpeckContainer() { }
        static public SpeckContainer Instance { get; } = new SpeckContainer();

        internal void InjectSingleton(Type type, Type referencedType)
        {
            var injectionModel = new InjectionModel(
                                     type: type,
                                     referencedType: referencedType);

            InjectionModels.Add(injectionModel);
        }

        internal void InjectSingleton(object formattedObject, Type referencedType)
        {
            var type = formattedObject.GetType();
            var injectionModel = new InjectionModel(type, formattedObject);
            InjectionModels.Add(injectionModel);
        }

        internal void InjectType<T>() => InjectType(typeof(T));

        internal void InjectType(Type type)
        {
            var injectionModel = new InjectionModel(
                                     type: type,
                                     injectionMode: Instantiation.PerRequest);

            InjectionModels.Add(injectionModel);
        }

        /// <summary>
        /// Retrieves a Speck via the T type used for lookup.
        /// </summary>
        /// <typeparam name="T">The type of Speck to retrieve</typeparam>
        /// <returns>Returns initialied Speck or throws exception is no Speck is of the requested type.</returns>
        public T GetInstance<T>(bool throwable = true) => (T)GetInstance(typeof(T), throwable);

        internal object GetInstance(Type type, bool throwable = true)
        {
            var injectionModel = InjectionModels.FirstOrDefault(model => model.Type == type || model.ReferencedType == type);

            switch (injectionModel?.InjectionMode)
            {
                case Instantiation.PerRequest:
                    var newSpeck = Activator.CreateInstance(injectionModel.Type);
                    return newSpeck;
                case Instantiation.Singleton:
                    return injectionModel.Instance;
                default:
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

        ~SpeckContainer()
        {
            foreach (var injectionModel in InjectionModels)
                if (injectionModel.Instance is IDisposable disposable)
                    try { disposable?.Dispose(); }
                    catch { }
        }
    }
}
