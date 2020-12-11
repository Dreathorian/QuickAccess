using System;

namespace QuickAccess.Keys {
    struct AttributeKey
    {
        public readonly Type AttributeType;
        public readonly Type OwningType;

        internal AttributeKey(Type owningType, Type attributeType)
        {
            OwningType = owningType;
            AttributeType = attributeType;
        }
    }
}