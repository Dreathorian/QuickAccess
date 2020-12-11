using System.Reflection;

namespace QuickAccess
{
    public partial class QuickAccess
    {
        public static T GetStaticValueAs<T>(this FieldInfo info)
        {
            return info.ReflectedType.StaticPropFieldGetter<T>(info.Name)();
        }
        public static TValue GetValueAs<TOwner, TValue>(this FieldInfo info, TOwner ownerObject)
        {
            return info.ReflectedType.PropFieldGetter<TOwner, TValue>(info.Name)(ownerObject);
        }
    }
}