using System;

namespace QuickAccess.Keys {
    class InterfaceDefinition
    {
        internal readonly bool has;

        internal readonly Type[] genericArguments;

        public InterfaceDefinition(bool has, Type[] genericArguments)
        {
            this.has = has;
            this.genericArguments = genericArguments;
        }
    }
}