using System;

namespace SpeckyStandard.Attributes
{
    /// <summary>
    /// A Speck used to declare a type to be used as a data access layer via rest.
    /// HttpClient and NewtonSoft is used to call the url provided and update the values given the RestDalAttribute. 
    /// </summary>
    public sealed class RestPollingAttribute : ContextBaseAttribute
    {
        private int interval = DefaultInterval;

        public RestPollingAttribute() { }
        public RestPollingAttribute(string headUrl)
        {
            HeadUrl = headUrl;
        }

        /// <summary>
        /// The url used by HttpClient to download the string.
        /// </summary>
        public string HeadUrl { get; }

        /// <summary>
        /// The polling interval used.
        /// </summary>
        public int Interval
        {
            get => interval;
            set
            {
                interval = value > 0
                         ? value
                         : throw new Exception($"{nameof(RestPollingAttribute)}.{nameof(Interval)} must be greater than 0.");
            }
        }

        /// <summary>
        /// The last time polling took place.
        /// </summary>
        public DateTime LastInterval { get; set; } = DateTime.MinValue;

        /// <summary>
        /// The default polling interval used if value is less than or equal to 0 or not provided.
        /// </summary>
        public const int DefaultInterval = 1000;
    }
}
