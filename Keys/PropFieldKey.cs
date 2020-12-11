using System;
using System.Reflection;

namespace QuickAccess.Keys {
    struct PropFieldKey
    {
        public readonly bool IsSet;

        public readonly bool IsStatic;

        public readonly Type PropFieldType;

        public readonly MemberInfo MemberInfo;

        public PropFieldKey(MemberInfo memberInfo, bool isStatic, bool isSet)
        {
            if (memberInfo == null) throw new ArgumentNullException();

            IsSet = isSet;
            IsStatic = isStatic;
            PropFieldType = GetPropFieldType(memberInfo);
            MemberInfo = memberInfo;
        }

        private static Type GetPropFieldType(MemberInfo mInfo)
        {
            if ((object) mInfo != null)
            {
                FieldInfo fieldInfo;
                if ((object) (fieldInfo = mInfo as FieldInfo) != null)
                {
                    FieldInfo fieldInfo2 = fieldInfo;
                    return fieldInfo2.FieldType;
                }

                PropertyInfo propertyInfo;
                if ((object) (propertyInfo = mInfo as PropertyInfo) != null)
                {
                    PropertyInfo propertyInfo2 = propertyInfo;
                    return propertyInfo2.PropertyType;
                }
            }

            throw new ArgumentException("case not covered");
        }
    }
}