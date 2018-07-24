using SpeckyStandard.Attributes;
using System;

namespace SpeckyStandard.Controllers
{
    internal interface IDalController<TDalAttribute> where TDalAttribute : ContextBaseAttribute
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
