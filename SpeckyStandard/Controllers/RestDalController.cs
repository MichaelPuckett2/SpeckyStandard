using Newtonsoft.Json;
using SpeckyStandard.Attributes;
using SpeckyStandard.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SpeckyStandard.Controllers
{
    internal class RestDalController : IDalController<RestDalAttribute>
    {
        private bool isStarted;
        private bool canContinue;
        private List<SpeckDal<RestDalAttribute>> RestSpeckDals { get; } = new List<SpeckDal<RestDalAttribute>>();

        private RestDalController() { }
        public static RestDalController Instance { get; } = new RestDalController();

        public event EventHandler Started;
        public event EventHandler Stopped;

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
            IsStarted = true;
            canContinue = true;

            var controllerLoop = Task.Factory.StartNew(() =>
            {
                while (canContinue)
                {
                    var restDalModels = RestSpeckDals.ToList();
                    foreach (var restDalModel in restDalModels)
                    {
                        //var dalProperties = restDalModel.Instance.GetType().GetProperties().Where(prop => prop.GetAttribute<RestDalAttribute>())
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Stop() => canContinue = false;

        public void Add(SpeckDal<RestDalAttribute> restSpeckDal)
        {
            if (IsStarted) throw new Exception($"Cannot add {nameof(SpeckDal<RestDalAttribute>)} while controller is started.");
            RestSpeckDals.Add(restSpeckDal);
        }

        public void Clear()
        {
            if (IsStarted) throw new Exception($"Cannot clear {nameof(RestSpeckDals)} while controller is started.");
            RestSpeckDals.Clear();
        }

        private Task<T> GetJsonResultAsync<T>(string url)
        {
            return Task.Run(() =>
            {
                var webClient = SpeckContainer.Instance.GetInstance<WebClient>();
                var json = webClient.DownloadString(url);
                return JsonConvert.DeserializeObject<T>(json);
            });
        }
    }
}
