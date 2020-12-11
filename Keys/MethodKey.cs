using System;

namespace QuickAccess.Keys {
    struct MethodKey
    {
        public readonly Type OwningType;

        public readonly string MethodName;

        public readonly Type[] ParameterTypes;

        public MethodKey(Type owningType, string methodName, Type[] parameterTypes)
        {
            OwningType = owningType;
            MethodName = methodName;
            ParameterTypes = parameterTypes;
        }
    }
}