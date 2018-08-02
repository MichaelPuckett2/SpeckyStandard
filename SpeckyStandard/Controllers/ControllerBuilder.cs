using SpeckyStandard.Attributes;
using SpeckyStandard.DI;
using SpeckyStandard.Enums;
using SpeckyStandard.Extensions;
using SpeckyStandard.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SpeckyStandard.Controllers
{
    internal static class ControllerBuilder
    {
        public static bool IsControllersBuilt { get; private set; }

        internal static void Start()
        {
            var speckDals = GetSpeckDals();
            BuildDalControllers(speckDals);
            IsControllersBuilt = true;
        }

        private static void BuildDalControllers(IEnumerable<SpeckDal<ContextBaseAttribute>> speckDals)
        {
            foreach (var speckDal in speckDals)
            {
                switch (speckDal.DalAttribute)
                {
                    case RestPollingAttribute restDal:
                        Log.Print($"Adding {nameof(SpeckDal<RestPollingAttribute>)}.", PrintType.DebugWindow);

                        RestDalController.Instance.Add(new SpeckDal<RestPollingAttribute>(speckDal.InjectionModel, restDal));
                        break;
                    default:
                        break;
                }
            }
        }

        private static IEnumerable<SpeckDal<ContextBaseAttribute>> GetSpeckDals()
        {
            return from speck in SpeckContainer.Instance.InjectionModels
                   let dal = speck.Type.GetAttribute<ContextBaseAttribute>()
                   where dal != null
                   select new SpeckDal<ContextBaseAttribute>(speck, dal);
        }
    }
}