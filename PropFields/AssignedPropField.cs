using System;
using System.Data;
using UnityEngine;

namespace QuickAccess.PropFields
{
    public class AssignedPropField<TOwner, TValue> : _PropField
    {
        private readonly Func<TOwner, TValue> _get;

        private readonly Action<TOwner, TValue> _set;

        public readonly TOwner AssignedObject;

        public AssignedPropField(Type fieldOwner, string fieldName, TOwner assignedObject)
            : base(fieldOwner, fieldName)
        {
            if (assignedObject == null) throw new ArgumentNullException();
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
            Type objectType = assignedObject.GetType();
            if (!objectType.IsSameOrSubclassOf(FieldOwner))
                throw new ArgumentException(
                    $"AssignedPropField: assignedObject Type ({objectType}) is not of Type {FieldOwner}");
            AssignedObject = assignedObject;
        }

        public TValue Get() => _get(AssignedObject);

        public sealed override bool CanSet { get; protected set; }

        public void Set(TValue value)
        {
            if (!CanSet && Application.isEditor)
                Debug.LogError($"AssignedPropField: Cannot set Field {FieldName} of {FieldOwner}. It is readonly.");
            else
                _set(AssignedObject, value);
        }
    }

    public class AssignedPropField : AssignedPropField<object, object>
    {
        public AssignedPropField(Type fieldOwner, string fieldName, object assignedObject) : base(fieldOwner, fieldName,
            assignedObject) { }
    }
}