using System;

namespace QuickAccess
{
    public partial class QuickAccess
    {
        public static Func<object[], TReturn> GetStaticFunc<TReturn>(this Type type, string methodName, params Type[] paramTypes)
        {
            return type.GetMethodInfo(methodName, paramTypes)?.CreateDelegate<Func<object[], TReturn>>(true);
        }
        public static Func<object[], object> GetStaticFunc(this Type type, string methodName, params Type[] paramTypes)
        {
            return type.GetStaticFunc<object>(methodName, paramTypes);
        }
        
        public static Func<TParam1, TParam2, TParam3, TReturn> GetStaticFunc<TParam1, TParam2, TParam3, TReturn>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2), typeof(TParam3))
                ?.CreateDelegate<Func<TParam1, TParam2, TParam3, TReturn>>(true);
        }
        public static Func<TParam1, TParam2, TReturn> GetStaticFunc<TParam1, TParam2, TReturn>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2))
                ?.CreateDelegate<Func<TParam1, TParam2, TReturn>>(true);
        }
        public static Func<TParam, TReturn> GetStaticFunc<TParam, TReturn>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam))
                ?.CreateDelegate<Func<TParam, TReturn>>(true);
        }
        public static Func<object, TReturn> GetStaticFunc<TReturn>(this Type type, string methodName, Type paramType)
        {
            return type.GetMethodInfo(methodName, paramType)?.CreateDelegate<Func<object, TReturn>>(true);
        }
        
        public static Func<TReturn> GetStaticFunc<TReturn>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName)?.CreateDelegate<Func<TReturn>>(true);
        }
        public static Func<object> GetStaticFunc(this Type type, string methodName)
        {
            return type.GetStaticFunc<object>(methodName);
        }

    }
}