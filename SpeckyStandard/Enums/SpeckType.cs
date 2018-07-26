namespace SpeckyStandard.Enums
{
    public enum SpeckType
    {
        /// <summary>
        /// Directs the Speck to act as a single instance.
        /// </summary>
        Singleton,
        /// <summary>
        /// Directs the Speck to act as a type and is initialized per request.
        /// </summary>
        PerRequest
    }
}
