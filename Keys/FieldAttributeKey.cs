using System;

namespace QuickAccess.Keys {
    class FieldAttributeKey
    {
        public readonly Type AttributeType;

        public readonly string FieldName;
        public readonly Type OwningType;

        internal FieldAttributeKey(Type owningType, string fieldName, Type attributeType)
        {
            OwningType = owningType;
            FieldName = fieldName;
            AttributeType = attributeType;
        }
    }
}