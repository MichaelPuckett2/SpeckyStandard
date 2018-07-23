using SpeckyStandard.Attributes;
using SpeckyStandard.DI;
using SpeckyStandard.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SpeckyStandard.Controllers
{
    internal sealed class ControllerBuilder
    {
        internal void Start()
        {
            var speckDals = GetSpeckDals();
            BuildDalControllers(speckDals);
        }

        private static void BuildDalControllers(IEnumerable<SpeckDal<DalBaseAttribute>> speckDals)
        {
            foreach (var speckDal in speckDals)
            {
                switch (speckDal.DalAttribute)
                {
                    case RestDalContextAttribute restDal:
                        RestDalController.Instance.Add(new SpeckDal<RestDalContextAttribute>(speckDal.InjectionModel, restDal));
                        break;
                    default:
                        break;
                }
            }
        }

        private static IEnumerable<SpeckDal<DalBaseAttribute>> GetSpeckDals()
        {
            return from speck in SpeckContainer.Instance.InjectionModels
                   let dal = speck.Type.GetAttribute<DalBaseAttribute>()
                   where dal != null
                   select new SpeckDal<DalBaseAttribute>(speck, dal);
        }
    }
}