using System;
using System.Reflection;

namespace QuickAccess.Keys
{
    struct MemberKey
    {
        public readonly Type OwningType;

        public readonly string MemberName;

        public FieldInfo FieldInfo => OwningType.GetFieldInfo(MemberName);

        public MemberKey(Type owningType, string memberName)
        {
            OwningType = owningType;
            MemberName = memberName;
        }

        public override string ToString()
        {
            return $"{OwningType}.{MemberName}";
        }
    }
}