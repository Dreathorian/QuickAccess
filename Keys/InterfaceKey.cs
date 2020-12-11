using System;

namespace QuickAccess.Keys {
    class InterfaceKey
    {
        internal readonly Type interfaceType;
        internal readonly Type owningType;

        public InterfaceKey(Type owningType, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException(interfaceType.Name + " is not an interface");

            this.owningType = owningType;
            this.interfaceType = interfaceType;
        }
    }
}