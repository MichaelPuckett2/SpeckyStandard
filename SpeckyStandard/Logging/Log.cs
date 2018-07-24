using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Logging
{
    internal static class Log
    {
        public static void Print(string message, PrintType printType, Exception innerException = null)
        {
            if (printType == PrintType.DebugWindow)
            {
                System.Diagnostics.Debug.Print(message);
            }

            if (printType == PrintType.LogFile)
            {
                throw new NotImplementedException($"{nameof(Log)}.{nameof(Print)} does not yet support {nameof(PrintType.LogFile)}");
            }

            if (printType == PrintType.ThrowException)
            {
                if (innerException != null)
                    throw innerException == null 
                        ? throw new Exception(message) 
                        : new Exception(message, innerException);
            }
        }
    }
}
