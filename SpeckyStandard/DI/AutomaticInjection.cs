using System.Reflection;

namespace SpeckyStandard.DI
{
    public static class AutomaticInjection
    {
        static volatile bool InjectionStarted = false;
        public static void Start()
        {
            if (InjectionStarted) return;
            InjectionStarted = true;
            var callingAssembly = Assembly.GetCallingAssembly();
            new AutoInjectioner(callingAssembly).Start();
        }
    }
}
