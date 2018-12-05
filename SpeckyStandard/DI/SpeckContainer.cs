using SpeckyStandard.Attributes;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        internal Type InjectSingleton(Type type)
        {
            var injectionModel = new InjectionModel(
                                     type: type,
                                     referencedType: type);

            InjectionModels.Add(injectionModel);
            return type;
        }

        internal Type InjectSingleton(Type type, Type referencedType, object[] instances)
        {
            InjectionModel injectionModel = instances.Any()
                                          ? new InjectionModel(
                                                type: type,
                                                referencedType: referencedType,
                                                parameters: instances)
                                          : new InjectionModel(
                                                type: type,
                                                referencedType: referencedType);

            InjectionModels.Add(injectionModel);
            return type;
        }

        internal Type InjectSingleton(object formattedObject, Type referencedType)
        {
            var type = formattedObject.GetType();
            var injectionModel = new InjectionModel(type, formattedObject);
            InjectionModels.Add(injectionModel);
            return type;
        }

        internal void InjectType<T>() => InjectType(typeof(T));

        internal void InjectType(Type type)
        {
            var injectionModel = new InjectionModel(
                                     type: type,
                                     injectionMode: SpeckType.PerRequest);

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
                case SpeckType.PerRequest:
                    var newSpeck = Activator.CreateInstance(injectionModel.Type);
                    return newSpeck;

                case SpeckType.Singleton:
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

        internal object[] GetInstances(ParameterInfo[] constructorParameters)
        {
            var objects = new List<object>();
            foreach (var parameterInfo in constructorParameters)
            {
                var speckValueAttribute = parameterInfo.GetCustomAttribute<SpeckValueAttribute>();
                if (speckValueAttribute != null)
                {
                    var configs = InjectionModels.Where(x => x.Type.GetAttribute<SpeckConfigurationAttribute>(att => att.Profile == GlobalConfiguration.Profile) != null);

                    object configValue = null;
                    foreach (var config in configs)
                    {
                        foreach (var prop in config.Type.GetProperties(Constants.BindingFlags))
                        {
                            if (prop.Name == speckValueAttribute.Key)
                            {
                                configValue = prop.GetValue(config.Instance);
                                break;
                            }
                        }

                        foreach (var field in config.Type.GetFields(Constants.BindingFlags))
                        {
                            if (field.Name == speckValueAttribute.Key)
                            {
                                configValue = field.GetValue(config.Instance);
                                break;
                            }
                        }
                    }

                    objects.Add(configValue
                             ?? throw new Exception($"{nameof(GetInstances)} didn't find value for key {speckValueAttribute.Key}.  Check the profile and configuration types to make sure the value was initialized."));
                }
                else
                {
                    var instance = (from injectionModel in InjectionModels
                                    where injectionModel.Type == parameterInfo.ParameterType || injectionModel.ReferencedType == parameterInfo.ParameterType
                                    select injectionModel.Instance)
                                   .FirstOrDefault();

                    objects.Add(instance
                             ?? throw new Exception($"{nameof(GetInstances)} didn't find instance for type {parameterInfo.ParameterType.Name}.  Make sure a Speck exists of that type."));
                }
            }
            return objects.ToArray();
        }

        internal object[] GetInstances(IEnumerable<Type> parameterTypes)
        {
            var objects = from injectionModel in InjectionModels
                          from parameterType in parameterTypes
                          where injectionModel.Type == parameterType
                             || injectionModel.ReferencedType == parameterType
                          select injectionModel.Instance;

            return objects.ToArray();
        }
    }
}
