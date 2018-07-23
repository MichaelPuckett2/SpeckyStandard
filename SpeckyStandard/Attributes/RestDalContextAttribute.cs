using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RestDalContextAttribute : DalBaseAttribute
    {
        public string Url { get; }
        public int Interval { get; }
        public DateTime LastInterval { get; set; } = DateTime.MinValue;
        public const int DefaultInterval = 1000;

        public RestDalContextAttribute(string url) : this(url, DefaultInterval) { }

        public RestDalContextAttribute(string url, int interval) 
        {
            Url = url;
            Interval = interval > 0 
                     ? interval 
                     : throw new Exception($"{nameof(RestDalContextAttribute)}.{nameof(Interval)} must be greater than 0.");
        }
    }
}
