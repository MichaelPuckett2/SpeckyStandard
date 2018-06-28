using SpeckyStandard.Attributes;
using SpeckyStandard.DI;
using System.Linq;

namespace SpeckyStandard.Controllers
{
    internal sealed class ControllerBuilder
    {
        internal void Start()
        {
            var speckDals = from speck in SpeckContainer.Instance.InjectionModels
                            let dal = speck.GetAttribute<DalBaseAttribute>()
                            where dal != null
                            select new SpeckDal<DalBaseAttribute>(speck, dal);
            
            foreach (var speckDal in speckDals)
            {
                switch (speckDal.DalAttribute)
                {
                    case RestDalAttribute restDal:
                        RestDalController.Instance.Add(new SpeckDal<RestDalAttribute>(speckDal.InjectionModel, restDal));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
