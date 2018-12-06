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
        internal Dictionary<string, object> ConfigurationModels { get; } = new Dictionary<string, object>();

        SpeckContainer() { }
        static public SpeckContainer Instance { get; } = new SpeckContainer();

        internal InjectionModel InjectSingleton(Type type)
        {
            var injectionModel = new InjectionModel(
                                     type: type,
                                     referencedType: type);

            InjectionModels.Add(injectionModel);
            return injectionModel;
        }

        internal InjectionModel InjectSingleton(Type type, Type referencedType, object[] instances)
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
            return injectionModel;
        }

        internal InjectionModel InjectSingleton(object formattedObject, Type referencedType)
        {
            var type = formattedObject.GetType();
            var injectionModel = new InjectionModel(type, formattedObject, referencedType);
            InjectionModels.Add(injectionModel);
            return injectionModel;
        }

        internal InjectionModel InjectType<T>() => InjectType(typeof(T));

        internal InjectionModel InjectType(Type type)
        {
            var injectionModel = new InjectionModel(
                                     type: type,
                                     injectionMode: SpeckType.PerRequest);

            InjectionModels.Add(injectionModel);
            return injectionModel;
        }

        internal void InjectConfiguration(Type type)
        {
            var config = Activator.CreateInstance(type);

            foreach (var propertyInfo in config.GetType().GetProperties(Constants.BindingFlags))
            {
                ConfigurationModels[propertyInfo.Name] = propertyInfo.GetValue(config);
            }

            foreach (var fieldInfo in config.GetType().GetFields(Constants.BindingFlags))
            {               
                ConfigurationModels[fieldInfo.Name] = fieldInfo.GetValue(config);
            }
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

        internal object GetConfigurationValue(string value)
        {
            return ConfigurationModels[value];
        }

        /// <summary>
        /// Looks to see if Specky has the speck type injected.  Useful to prevent initializing specks of type PerRequest
        /// </summary>
        /// <param name="type">Speck Type to look up</param>
        /// <returns></returns>
        public bool HasSpeck(Type type)
        {
            return InjectionModels.FirstOrDefault(model => model.Type == type || model.ReferencedType == type) != null;
        }

        ~SpeckContainer()
        {
            foreach (var injectionModel in InjectionModels)
                if (injectionModel.Instance is IDisposable disposable)
                    try { disposable?.Dispose(); }
                    catch { }
        }

        internal object[] GetParameterInstances(ParameterInfo[] constructorParameters)
        {
            var parameterInstances = new List<object>();
            foreach (var parameterInfo in constructorParameters)
            {
                if (parameterInfo.TryGetAttribute(out SpeckConfigurationAutoAttribute speckConfigurationAutoAttribute))
                {
                    var configInjectionModels = InjectionModels.Where(x => x.Type.GetAttribute<SpeckConfigurationAttribute>(att => att.Configuration == GlobalConfiguration.Profile) != null);

                    object configInstance = null;
                    foreach (var configInjectionModel in configInjectionModels)
                    {
                        foreach (var configProperty in configInjectionModel.Type.GetProperties(Constants.BindingFlags))
                        {
                            if (configProperty.Name == speckConfigurationAutoAttribute.Key)
                            {
                                configInstance = configProperty.GetValue(configInjectionModel.Instance);
                                break;
                            }
                        }

                        if (configInstance != null) continue;

                        foreach (var field in configInjectionModel.Type.GetFields(Constants.BindingFlags))
                        {
                            if (field.Name == speckConfigurationAutoAttribute.Key)
                            {
                                configInstance = field.GetValue(configInjectionModel.Instance);
                                break;
                            }
                        }
                    }

                    parameterInstances.Add(configInstance
                             ?? throw new Exception($"{nameof(GetParameterInstances)} didn't find value for key {speckConfigurationAutoAttribute.Key}.  Check the profile and configuration types to make sure the value was initialized."));
                }
                else
                {
                    var instance = (from injectionModel in InjectionModels
                                    where injectionModel.Type == parameterInfo.ParameterType
                                       || injectionModel.ReferencedType == parameterInfo.ParameterType
                                    select injectionModel.Instance)
                                   .FirstOrDefault();

                    parameterInstances.Add(instance
                             ?? throw new Exception($"{nameof(GetParameterInstances)} didn't find instance for type {parameterInfo.ParameterType.Name}.  Make sure a Speck exists of that type."));
                }
            }
            return parameterInstances.ToArray();
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
