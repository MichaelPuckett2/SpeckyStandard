using Newtonsoft.Json;
using SpeckyStandard.Attributes;
using SpeckyStandard.DI;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using SpeckyStandard.Logging;
using SpeckyStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SpeckyStandard.Controllers
{
    internal class RestDalController : IDalController<SpeckRestPollingAttributeAttribute>
    {
        private volatile bool isStarted;
        private volatile bool canContinue;
        public event EventHandler Started;
        public event EventHandler Stopped;

        private List<SpeckDal<SpeckRestPollingAttributeAttribute>> RestSpeckDals { get; } = new List<SpeckDal<SpeckRestPollingAttributeAttribute>>();

        private RestDalController() { }
        public static RestDalController Instance { get; } = new RestDalController();

        public bool CanStart => RestSpeckDals.Any() && !IsStarted;

        public bool IsStarted
        {
            get => isStarted;
            private set
            {
                if (isStarted == value) return;
                isStarted = value;
                if (isStarted)
                    Started?.Invoke(this, EventArgs.Empty);
                else
                    Stopped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Start()
        {
            if (!CanStart)
                throw new Exception($"{nameof(RestDalController)}.{nameof(Start)} cannot start either because it has already started or there is no data available for work. Please check {nameof(CanStart)} before calling start.");

            Log.Print($"Starting {nameof(RestDalController)}.", PrintType.DebugWindow);

            InsureHttpClientInstance();

            IsStarted = true;
            canContinue = true;
            StartControllerLoop();
        }

        public void Stop() => canContinue = false;

        public void Add(SpeckDal<SpeckRestPollingAttributeAttribute> restSpeckDal)
        {
            if (IsStarted) throw new Exception($"Cannot add {nameof(SpeckDal<SpeckRestPollingAttributeAttribute>)} while controller is started.");
            RestSpeckDals.Add(restSpeckDal);
        }

        public void Clear()
        {
            if (IsStarted) throw new Exception($"Cannot clear {nameof(RestSpeckDals)} while controller is started.");
            RestSpeckDals.Clear();
        }

        private static void InsureHttpClientInstance()
        {
            if (SpeckContainer.Instance.GetInstance<HttpClient>(false) == null)
                SpeckContainer.Instance.InjectType(typeof(HttpClient));
        }

        private void StartControllerLoop()
        {
            Task.Factory.StartNew(() =>
            {
                while (canContinue)
                {
                    ProcessRestDalContexts();
                    Task.Delay(10).Wait();
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void ProcessRestDalContexts()
        {
            foreach (var restDalModel in RestSpeckDals.ToList())
            {
                if (!canContinue) break;
                ProcessRestDalContext(restDalModel);
            }
        }

        private void ProcessRestDalContext(SpeckDal<SpeckRestPollingAttributeAttribute> restDalModel)
        {
            if (restDalModel.DalAttribute.LastInterval.IsNowPast(restDalModel.DalAttribute.Interval))
            {
                var restDals = from propertyInfo in restDalModel.InjectionModel.Instance.GetType().GetProperties()
                               let restDal = propertyInfo.GetAttribute<SpeckRestDataAttribute>()
                               where restDal != null
                               select new { PropInfo = propertyInfo, RestDal = restDal };

                foreach (var restDal in restDals)
                {
                    var url = $"{restDalModel.DalAttribute.HeadUrl}{restDal.RestDal.Url}";
                    var dalResult = GetJsonResult(url, restDal.PropInfo.PropertyType);

                    try
                    {
                        var setMethod = restDal.PropInfo.GetSetMethod(true);
                        setMethod.Invoke(restDalModel.InjectionModel.Instance, new object[] { dalResult });

                        if (restDalModel.InjectionModel.Instance is NotifyBase notifyBase
                        && restDal.RestDal.CanNotify)
                        {
                            var propertyName = restDal.PropInfo.Name;
                            Log.Print($"Notify: {propertyName}", PrintType.DebugWindow);
                            notifyBase.Notify(propertyName);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Print(exception.Message, DebugSettings.DebugPrintType, exception);
                    }
                }

                restDalModel.DalAttribute.LastInterval = DateTime.Now;
            }
        }

        private object GetJsonResult(string url, Type type)
        {
            try
            {
                Log.Print($"{nameof(HttpClient)} at {url}", PrintType.DebugWindow);
                var httpClient = SpeckContainer.Instance.GetInstance<HttpClient>();
                var json = httpClient.GetStringAsync(url).Result;
                Log.Print($"Result:\n{json}", PrintType.DebugWindow);
                return JsonConvert.DeserializeObject(json, type);
            }
            catch (Exception exception)
            {
                Log.Print(exception.Message, DebugSettings.DebugPrintType, exception);
                return null;
            }
        }
    }
}
