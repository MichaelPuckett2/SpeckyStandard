using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public abstract class DalBaseAttribute : Attribute { }
}
