using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class WebDAL : Attribute
    {
        public string Url { get; }
        public int Interval { get; }

        public WebDAL(string url) : this(url, 0) { }

        public WebDAL(string url, int interval) 
        {
            Url = url;
            Interval = interval;
        }
    }
}
