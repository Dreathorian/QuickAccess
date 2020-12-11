using System;

namespace QuickAccess
{
    public partial class QuickAccess
    {
        public static Action<object[]> GetStaticAction(this Type type, string methodName, params Type[] paramTypes)
        {
            return type.GetMethodInfo(methodName, paramTypes)?.CreateDelegate<Action<object[]>>(true);
        }

        public static Action<object> GetStaticAction(this Type type, string methodName, Type paramType)
        {
            return type.GetMethodInfo(methodName, paramType)?.CreateDelegate<Action<object>>(true);
        }

        public static Action<TParam1, TParam2, TParam3> GetStaticAction<TParam1, TParam2, TParam3>(this Type type,
            string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2), typeof(TParam3))?
                .CreateDelegate<Action<TParam1, TParam2, TParam3>>(true);
        }

        public static Action<TParam1, TParam2> GetStaticAction<TParam1, TParam2>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2))?
                .CreateDelegate<Action<TParam1, TParam2>>(true);
        }

        public static Action<TParam> GetStaticAction<TParam>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam))?
                .CreateDelegate<Action<TParam>>(true);
        }

        public static Action GetStaticAction(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName)?.CreateDelegate<Action>(true);
        }
    }
}