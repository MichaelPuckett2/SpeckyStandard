using SpeckyStandard.Enums;

namespace SpeckyStandard.Logging
{
    public static class DebugSettings
    {
        public static PrintType DebugPrintType { get; set; } = PrintType.DebugWindow | PrintType.ThrowException;
    }
}
