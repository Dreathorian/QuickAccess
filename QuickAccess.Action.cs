using System;

namespace QuickAccess
{
    public partial class QuickAccess
    {
        public static Action<object, object[]> GetAction(this Type type, string methodName,
            params Type[] paramTypes)
        {
            return type.GetAction<object>(methodName, paramTypes);
        }
        public static Action<object, object> GetAction(this Type type, string methodName, Type paramType)
        {
            return type.GetAction<object>(methodName, paramType);
        }
        public static Action<object> GetAction(this Type type, string methodName)
        {
            return type.GetAction<object>(methodName);
        }
        public static Action<TObject, object[]> GetAction<TObject>(this Type type, string methodName,
            params Type[] paramTypes)
        {
            return type.GetMethodInfo(methodName, paramTypes)?.CreateDelegate<Action<TObject, object[]>>(false);
        }
        public static Action<TObject, TParam> GetAction<TObject, TParam>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam))?.CreateDelegate<Action<TObject, TParam>>(false);
        }
        public static Action<TObject, TParam1, TParam2> GetAction<TObject, TParam1, TParam2>(this Type type,
            string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2))
                ?.CreateDelegate<Action<TObject, TParam1, TParam2>>(false);
        }
        public static Action<TObject, TParam1, TParam2, TParam3> GetAction<TObject, TParam1, TParam2, TParam3>(
            this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2), typeof(TParam3))
                ?.CreateDelegate<Action<TObject, TParam1, TParam2, TParam3>>(false);
        }
        public static Action<TObject, object> GetAction<TObject>(this Type type, string methodName, Type paramType)
        {
            return type.GetMethodInfo(methodName, paramType)?.CreateDelegate<Action<TObject, object>>(false);
        }
        public static Action<TObject> GetAction<TObject>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName)?.CreateDelegate<Action<TObject>>(false);
        }
    }
}