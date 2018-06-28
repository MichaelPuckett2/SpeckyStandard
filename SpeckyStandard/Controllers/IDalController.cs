using SpeckyStandard.Attributes;
using System;

namespace SpeckyStandard.Controllers
{
    internal interface IDalController<TDalAttribute> where TDalAttribute : DalBaseAttribute
    {
        bool IsStarted { get; }
        event EventHandler Started;
        event EventHandler Stopped;
        void Start();
        void Stop();
        void Add(SpeckDal<TDalAttribute> speckDal);
        void Clear();
    }
}
