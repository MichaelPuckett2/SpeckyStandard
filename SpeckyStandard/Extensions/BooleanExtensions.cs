using System;

namespace SpeckyStandard.Extensions
{
    public static class BooleanExtensions
    {
        /// <summary>
        /// Invokes seperate actions when boolean is true or false.
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="True">Action to invoke when boolean is true.</param>
        /// <param name="False">Action to invoke when boolean is false.</param>
        public static bool Pulse(this bool boolean, Action True = null, Action False = null)
        {
            if (boolean)
            {
                True?.Invoke();
            }
            else
            {
                False?.Invoke();
            }
            return boolean;
        }

        /// <summary>
        /// Invokes seperate predicates when boolean is true or false.
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="True">Predicate to invoke when boolean is true.  The result of the predicate will be the return value of the Pulse. If no predicate is supplied it will return the original boolean value.</param>
        /// <param name="False">Predicate to invoke when boolean is false. The result of the predicate will be the return value of the Pulse. If no predicate is supplied it will return the original boolean value.</param>
        /// <returns>Returns the result of the invoked predicate assocaited with if the boolean is true or false, if a predicate is supplied, otherwise returns the original boolean value.</returns>
        public static bool Pulse(this bool boolean, Predicate<bool> True = null, Predicate<bool> False = null)
        {
            if (boolean)
            {
                return True?.Invoke(boolean) ?? boolean;
            }
            else
            {
                return False?.Invoke(boolean) ?? boolean;
            }
        }
    }
}
