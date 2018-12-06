using System;
using System.Reflection;

namespace SpeckyStandard.Exceptions
{
    /// <summary>
    /// Exception indicates Specky was unable to apply one Speck to another Speck.
    /// </summary>
    public class SpeckMissingException : Exception
    {
        internal SpeckMissingException(Type speckType, Type requestedType, PropertyInfo targetProperty)
            : base($"{speckType.Name} - Speck of type {requestedType.Name} is missing and cannot be applied to {targetProperty.Name}") { }

        internal SpeckMissingException(Type speckType, Type requestedType, FieldInfo targetField)
            : base($"{speckType.Name} - Speck of type {requestedType.Name} is missing and cannot be applied to {targetField.Name}") { }

        internal SpeckMissingException(Type speckType, Type requestedType, MethodInfo targetMethod, ParameterInfo targetParameter)
            : base($"{speckType.Name} - Speck of type {requestedType.Name} is missing and cannot be passed to method {targetMethod.Name} as parameter {targetParameter.Name}") { }
    }
}
