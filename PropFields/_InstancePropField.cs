using System;

namespace QuickAccess.PropFields
{
    public abstract class _InstancePropField : _PropField
    {
        public abstract object WeakGet(object Owner);
        public abstract void WeakSet(object Owner, object value);
        protected _InstancePropField(Type type, string fieldName) : base(type, fieldName) { }
    }
}