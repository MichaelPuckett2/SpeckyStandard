using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RestDalAttribute : Attribute
    {
        public string Url { get; }

        public RestDalAttribute() { }
        public RestDalAttribute(string url)
        {
            Url = url ?? string.Empty;
        }
    }
}
