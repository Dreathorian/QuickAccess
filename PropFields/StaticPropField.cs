using System;
using System.Data;
using UnityEngine;

namespace QuickAccess.PropFields
{
    public class StaticPropField<TValue> : _PropField
    {
        private readonly Func<TValue> _get;

        private readonly Action<TValue> _set;

        public StaticPropField(Type type, string fieldName)
            : base(type, fieldName)
        {
            _get = type.StaticPropFieldGetter<TValue>(fieldName);
            try
            {
                _set = type.StaticPropFieldSetter<TValue>(fieldName);
            }
            catch (ReadOnlyException)
            {
                _set = null;
            }

            CanSet = _set != null;
        }

        public object Get => _get();

        public sealed override bool CanSet { get; protected set; }

        public void Set(TValue value)
        {
            if (!CanSet && Application.isEditor)
                Debug.LogError(string.Format("{0}: Cannot set Field {1} of Type {2}. ", "StaticPropField", FieldName,
                                   FieldOwner) + "It is readonly.");
            else
                _set(value);
        }
    }
}