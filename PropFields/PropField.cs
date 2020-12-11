using System;
using System.Data;
using UnityEngine;

namespace QuickAccess.PropFields
{
    public class PropField<TOwner, TValue> : _InstancePropField
    {
        private readonly Func<TOwner, TValue> _get;
        private readonly Action<TOwner, TValue> _set;

        public PropField(Type fieldOwner, string fieldName) : base(fieldOwner, fieldName)
        {
            _get = fieldOwner.PropFieldGetter<TOwner, TValue>(fieldName);
            try
            {
                _set = fieldOwner.PropFieldSetter<TOwner, TValue>(fieldName);
            }
            catch (ReadOnlyException)
            {
                _set = null;
            }

            CanSet = _set != null;
        }

        public override bool CanSet { get; protected set; }

        public TValue Get(TOwner owner) => _get(owner);

        public override object WeakGet(object Owner)
        {
            try
            {
                return Get((TOwner) Owner);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        public void Set(TOwner owner, TValue value)
        {
            if (!CanSet)
            {
                if (Application.isEditor)
                    Debug.LogError(
                        $"{nameof(_PropField)}: Cannot set Field {FieldName} of {FieldOwner}. It is readonly.");
            }
            else
                _set(owner, value);
        }

        public override void WeakSet(object Owner, object value)
        {
            try
            {
                Set((TOwner) Owner, (TValue) value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    // public class PropField : _PropField
    // {
    //     private readonly Func<object, object> _get;
    //
    //     private readonly Action<object, object> _set;
    //
    //     public PropField(Type fieldOwner, string fieldName)
    //         : base(fieldOwner, fieldName)
    //     {
    //         _get = fieldOwner.PropFieldGetter(fieldName);
    //         try
    //         {
    //             _set = fieldOwner.PropFieldSetter(fieldName);
    //         }
    //         catch (ReadOnlyException)
    //         {
    //             _set = null;
    //         }
    //
    //         CanSet = _set != null;
    //     }
    //
    //     public sealed override bool CanSet { get; protected set; }
    //
    //     public object Get(object owner)
    //     {
    //         return _get(owner);
    //     }
    //
    //     public void Set(object owner, object value)
    //     {
    //         if (!CanSet)
    //         {
    //             if (Application.isEditor)
    //                 Debug.LogError($"{nameof(PropField)}: Cannot set Field {FieldName} of {FieldOwner}. It is readonly.");
    //         }
    //         else
    //             _set(owner, value);
    //     }
    // }
}