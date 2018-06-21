using System;

namespace SpeckyStandard.Extensions
{
    public static class PrimitiveExtensions
    {
        /// <summary>
        /// Invokes an action when boolean is true.
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="action">Action to invoke when boolean is true.</param>
        public static void PulseOnTrue(this bool boolean, Action action)
        {
            if (boolean) action.Invoke();
        }

        /// <summary>
        /// Invokes an action when boolean is false.
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="action">Action to invoke when boolean is false.</param>
        public static void PulseOnFalse(this bool boolean, Action action)
        {
            if (!boolean) action.Invoke();
        }

        /// <summary>
        /// Invokes seperate actions when boolean is true or false.
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="trueAction">Action to invoke when boolean is true.</param>
        /// <param name="falseAction">Action to invoke when boolean is false.</param>
        public static void Pulse(this bool boolean, Action trueAction = null, Action falseAction = null)
        {
            if (boolean) boolean.PulseOnTrue(trueAction);
            else boolean.PulseOnFalse(falseAction);
        }

        /// <summary>
        /// Invokes a predicate when boolean is true.
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="predicate">Predicate to invoke when boolean is true.</param>
        /// <returns>Returns the result of the invoked predicate.</returns>
        public static bool PulseOnTrue(this bool boolean, Predicate<bool> predicate)
        {
            if (boolean) return predicate.Invoke(boolean); else return boolean;
        }

        /// <summary>
        /// Invokes a predicate when boolean is false.
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="predicate">Predicate to invoke when boolean is false.</param>
        /// <returns>Returns the result of the invoked predicate.</returns>
        public static bool PulseOnFalse(this bool boolean, Predicate<bool> predicate)
        {
            if (!boolean) return predicate.Invoke(boolean); else return boolean;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="boolean">Invokes seperate predicates when boolean is true or false.</param>
        /// <param name="truePredicate">Predicate to invoke when boolean is true.</param>
        /// <param name="falsePredicate">Predicate to invoke when boolean is false.</param>
        /// <returns>Returns the result of the invoked predicate.</returns>
        public static bool Pulse(this bool boolean, Predicate<bool> truePredicate = null, Predicate<bool> falsePredicate = null)
        {
            if (boolean) return boolean.PulseOnTrue(truePredicate);
            else return boolean.PulseOnFalse(falsePredicate);
        }
    }
}
