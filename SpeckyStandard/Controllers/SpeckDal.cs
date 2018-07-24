using SpeckyStandard.Attributes;
using SpeckyStandard.DI;

namespace SpeckyStandard.Controllers
{
    internal class SpeckDal<TDalAttribute> where TDalAttribute : ContextBaseAttribute
    {
        internal SpeckDal(InjectionModel injectionModel, TDalAttribute dalAttribute)
        {
            InjectionModel = injectionModel;
            DalAttribute = dalAttribute;
        }
        internal InjectionModel InjectionModel { get; }
        internal TDalAttribute DalAttribute { get; }
    }
}
