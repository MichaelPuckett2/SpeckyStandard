using SpeckyStandard.Enums;
using System;
using System.Collections.Generic;
using static SpeckyStandard.Logging.Log;

namespace SpeckyStandard.Extensions
{
    public static class FluentSpecky
    {
        public static TEnumerable WithEach<TEnumerable, T>(this TEnumerable items, Action<T> action) where TEnumerable : IEnumerable<T>
        {
            foreach (var item in items) action.Invoke(item);
            return items;
        }

        public static T Log<T>(this T t, string message, PrintType printType)
        {
            Print(message, printType);
            return t;
        }
    }
}
