using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class SpeckRestDataAttribute : Attribute
    {
        public string Url { get; }
        public bool CanNotify { get; set; } = true;
        public SpeckRestDataAttribute() { }
        public SpeckRestDataAttribute(string url)
        {
            Url = url;
        }
    }
}
