using System;

namespace QuickAccess
{
    public partial class QuickAccess
    {
        public static Func<object, object[], object> GetFunc(this Type type, string methodName,
            params Type[] paramTypes)
        {
            return type.GetFunc<object, object>(methodName, paramTypes);
        }

        public static Func<object, object, object> GetFunc(this Type type, string methodName, Type paramType)
        {
            return type.GetFunc<object, object>(methodName, paramType);
        }

        public static Func<object, object> GetFunc(this Type type, string methodName)
        {
            return type.GetFunc<object, object>(methodName);
        }

        public static Func<object, object[], TReturn> GetFunc<TReturn>(this Type type, string methodName,
            params Type[] paramTypes)
        {
            return type.GetFunc<object, TReturn>(methodName, paramTypes);
        }

        public static Func<object, object, TReturn> GetFunc<TReturn>(this Type type, string methodName, Type paramType)
        {
            return type.GetFunc<object, TReturn>(methodName, paramType);
        }

        public static Func<object, TReturn> GetFunc<TReturn>(this Type type, string methodName)
        {
            return type.GetFunc<object, TReturn>(methodName);
        }

        public static Func<TObject, object[], TReturn> GetFunc<TObject, TReturn>(this Type type, string methodName,
            params Type[] paramTypes)
        {
            return type.GetMethodInfo(methodName, paramTypes)?
                .CreateDelegate<Func<TObject, object[], TReturn>>(false);
        }

        public static Func<TObject, object, TReturn> GetFunc<TObject, TReturn>(this Type type, string methodName,
            Type paramType)
        {
            return type.GetMethodInfo(methodName, paramType)?
                .CreateDelegate<Func<TObject, object, TReturn>>(false);
        }

        public static Func<TObject, TReturn> GetFunc<TObject, TReturn>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName)?.CreateDelegate<Func<TObject, TReturn>>(false);
        }

        public static Func<TObject, TParam, TReturn> GetFunc<TObject, TParam, TReturn>(this Type type,
            string methodName)
        {
            return GetMethodInfo(type, methodName, typeof(TParam))?
                .CreateDelegate<Func<TObject, TParam, TReturn>>(false);
        }

        public static Func<TObject, TParam1, TParam2, TReturn> GetFunc<TObject, TParam1, TParam2, TReturn>(
            this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2))?
                .CreateDelegate<Func<TObject, TParam1, TParam2, TReturn>>(false);
        }

        public static Func<TObject, TParam1, TParam2, TParam3, TReturn> GetFunc<TObject, TParam1, TParam2, TParam3,
            TReturn>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2), typeof(TParam3))?
                .CreateDelegate<Func<TObject, TParam1, TParam2, TParam3, TReturn>>(false);
        }
    }
}