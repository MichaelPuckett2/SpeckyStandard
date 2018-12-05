using SpeckyStandard.Enums;
using System;

namespace SpeckyStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class SpeckLogAttribute : Attribute
    {
        public SpeckLogAttribute(string message, PrintType printType)
        {
            Message = message;
            PrintType = printType;
        }
        public PrintType PrintType { get; }
        public string Message { get; }
    }
}
