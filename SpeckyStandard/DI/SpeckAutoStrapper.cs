using System.Reflection;

namespace SpeckyStandard.DI
{
    /// <summary>
    /// Straps the application and injects all dependencies.
    /// Must be called at the applications point of entry and cannot be within a type inteded as a Speck.
    /// </summary>
    public static class SpeckAutoStrapper
    {
        static volatile bool InjectionStarted = false;

        /// <summary>
        /// Starts the strapping and injection process.
        /// </summary>
        public static void Start()
        {
            if (InjectionStarted) return;
            InjectionStarted = true;
            var callingAssembly = Assembly.GetCallingAssembly();
            new InjectionWorker(callingAssembly).Start();
        }
    }
}
