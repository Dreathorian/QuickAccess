using System;

namespace QuickAccess.PropFields
{
    public abstract class _PropField
    {
        public readonly string FieldName;

        public readonly Type FieldOwner;
        private Type _fieldType;

        internal _PropField(Type type, string fieldName)
        {
            if (type == null || fieldName == null) throw new ArgumentNullException();
            FieldOwner = type;
            FieldName = fieldName;
        }

        public Type FieldType => _fieldType ?? (_fieldType = FieldOwner.GetFieldInfo(FieldName).FieldType);

        public abstract bool CanSet { get; protected set; }


    }
}