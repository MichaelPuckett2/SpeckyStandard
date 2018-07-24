using Newtonsoft.Json;
using SpeckyStandard.Attributes;
using SpeckyStandard.Logging;
using SpeckyStandard.DI;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using SpeckyStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SpeckyStandard.Controllers
{
    internal class RestDalController : IDalController<RestPollingAttribute>
    {
        private volatile bool isStarted;
        private volatile bool canContinue;
        public event EventHandler Started;
        public event EventHandler Stopped;

        private List<SpeckDal<RestPollingAttribute>> RestSpeckDals { get; } = new List<SpeckDal<RestPollingAttribute>>();

        private RestDalController() { }
        public static RestDalController Instance { get; } = new RestDalController();

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
            if (IsStarted) throw new Exception($"{nameof(RestDalController)}.{nameof(Start)} called while already started.");

            InsureWebClientInstance();

            IsStarted = true;
            canContinue = true;
            StartControllerLoop();
        }

        public void Stop() => canContinue = false;

        public void Add(SpeckDal<RestPollingAttribute> restSpeckDal)
        {
            if (IsStarted) throw new Exception($"Cannot add {nameof(SpeckDal<RestPollingAttribute>)} while controller is started.");
            RestSpeckDals.Add(restSpeckDal);
        }

        public void Clear()
        {
            if (IsStarted) throw new Exception($"Cannot clear {nameof(RestSpeckDals)} while controller is started.");
            RestSpeckDals.Clear();
        }

        private static void InsureWebClientInstance()
        {
            if (SpeckContainer.Instance.GetInstance<WebClient>(false) == null)
                SpeckContainer.Instance.InjectType(typeof(WebClient));
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
            }, TaskCreationOptions.LongRunning);
        }

        private void ProcessRestDalContexts()
        {
            foreach (var restDalModel in RestSpeckDals.ToList())
            {
                if (!canContinue) break;
                ProcessRestDalContext(restDalModel);
            }
        }

        private void ProcessRestDalContext(SpeckDal<RestPollingAttribute> restDalModel)
        {
            if (restDalModel.DalAttribute.LastInterval.IsNowPast(restDalModel.DalAttribute.Interval))
            {
                var restDals = from propertyInfo in restDalModel.InjectionModel.Instance.GetType().GetProperties()
                               let restDal = propertyInfo.GetAttribute<RestDataAttribute>()
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
                Log.Print($"{nameof(WebClient)} at {url}", PrintType.DebugWindow);
                var webClient = SpeckContainer.Instance.GetInstance<WebClient>();
                var json = webClient.DownloadString(url);
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
