using System;

namespace QuickAccess.Keys
{
    class AssignedPropFieldKey
    {
        internal readonly object AssignedObject;

        internal readonly string FieldName;
        internal readonly Type OwningType;

        public AssignedPropFieldKey(Type owningType, string fieldName, object assignedObject)
        {
            OwningType = owningType;
            FieldName = fieldName;
            AssignedObject = assignedObject;
        }
    }
}