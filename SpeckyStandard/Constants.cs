using System.Reflection;

namespace SpeckyStandard
{
    internal static class Constants
    {
        internal static BindingFlags BindingFlags = BindingFlags.Instance 
                                                  | BindingFlags.Public 
                                                  | BindingFlags.NonPublic;
    }
}
