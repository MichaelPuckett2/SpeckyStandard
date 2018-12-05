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

        private static void BuildDalControllers(IEnumerable<SpeckDal<SpeckContextBaseAttribute>> speckDals)
        {
            foreach (var speckDal in speckDals)
            {
                switch (speckDal.DalAttribute)
                {
                    case SpeckRestPollingAttributeAttribute restDal:
                        Log.Print($"Adding {nameof(SpeckDal<SpeckRestPollingAttributeAttribute>)}.", PrintType.DebugWindow);
                        RestDalController.Instance.Add(new SpeckDal<SpeckRestPollingAttributeAttribute>(speckDal.InjectionModel, restDal));
                        break;
                    default:
                        break;
                }
            }
        }

        private static IEnumerable<SpeckDal<SpeckContextBaseAttribute>> GetSpeckDals()
        {
            return from speck in SpeckContainer.Instance.InjectionModels
                   let dal = speck.Type.GetAttribute<SpeckContextBaseAttribute>()
                   where dal != null
                   select new SpeckDal<SpeckContextBaseAttribute>(speck, dal);
        }
    }
}