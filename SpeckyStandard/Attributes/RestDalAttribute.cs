using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class RestDalAttribute : DalBaseAttribute
    {
        public string Url { get; }
        public int Interval { get; }

        public RestDalAttribute(string url) : this(url, 0) { }

        public RestDalAttribute(string url, int interval) 
        {
            Url = url;
            Interval = interval;
        }
    }
}
