using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class RestDataAttribute : Attribute
    {
        public string Url { get; }
        public bool CanNotify { get; set; } = true;
        public RestDataAttribute() { }
        public RestDataAttribute(string url)
        {
            Url = url;
        }
    }
}
