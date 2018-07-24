using SpeckyStandard.Controllers;
using SpeckyStandard.DI;
using System.Reflection;

namespace SpeckyStandard
{
    /// <summary>
    /// Straps the application and injects all dependencies.
    /// Must be called at the applications point of entry and cannot be within a type inteded as a Speck.
    /// </summary>
    public static class SpeckAutoStrapper
    {
        internal static bool IsStrappingStarted = false;
        internal static bool IsStrappingComplete = false;

        /// <summary>
        /// Starts the strapping and injection process.
        /// Note: It is important that this is performed first in your application and in the main application threading context.
        /// </summary>
        public static void Start()
        {
            if (IsStrappingStarted) return;
            IsStrappingStarted = true;
            var callingAssembly = Assembly.GetCallingAssembly();
            new InjectionWorker(callingAssembly).Start();
            new ControllerBuilder().Start();
            IsStrappingComplete = true;
        }
    }
}
