namespace SpeckyStandard.Enums
{
    public enum Instantiation
    {
        /// <summary>
        /// Directs the injected dependency to act as a single instance.
        /// </summary>
        Singleton,
        /// <summary>
        /// Directs the injected dependency to act as a type and is initialized per request.
        /// </summary>
        PerRequest
    }
}
