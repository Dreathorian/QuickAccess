using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using QuickAccess.Keys;
using QuickAccess.PropFields;
using UnityEngine;

namespace QuickAccess
{
    public static partial class QuickAccess
    {
        private static readonly Dictionary<MethodBase, IReadOnlyList<Type>> ParameterMap =
            new Dictionary<MethodBase, IReadOnlyList<Type>>();

        private static readonly Dictionary<MemberKey, MemberInfo> memberInfos = new Dictionary<MemberKey, MemberInfo>();

        private static readonly Dictionary<MethodKey, MethodBase> specificMethods =
            new Dictionary<MethodKey, MethodBase>();

        private static readonly Dictionary<AssignedPropFieldKey, _PropField> assignedPropFields =
            new Dictionary<AssignedPropFieldKey, _PropField>();

        private static readonly Dictionary<MemberKey, _PropField> propFields = new Dictionary<MemberKey, _PropField>();

        private static readonly Dictionary<PropFieldKey, Delegate> propFieldDelegates =
            new Dictionary<PropFieldKey, Delegate>();

        private static readonly Dictionary<AttributeKey, bool> hasClassAttributeDict =
            new Dictionary<AttributeKey, bool>();

        private static readonly Dictionary<AttributeKey, Array> classAttributesDict =
            new Dictionary<AttributeKey, Array>();

        private static readonly Dictionary<Type, Type[]> interfaceDict = new Dictionary<Type, Type[]>();

        private static readonly Dictionary<InterfaceKey, bool> hasInterfaceDict = new Dictionary<InterfaceKey, bool>();

        private static readonly Dictionary<InterfaceKey, InterfaceDefinition> hasInterfaceDefDict =
            new Dictionary<InterfaceKey, InterfaceDefinition>();

        private static readonly Dictionary<FieldAttributeKey, bool> hasFieldAttributeDict =
            new Dictionary<FieldAttributeKey, bool>();

        private static readonly Dictionary<MemberKey, Attribute[]> fieldAttributeDict =
            new Dictionary<MemberKey, Attribute[]>();

        private static readonly Dictionary<FieldAttributeKey, object> fieldAttributeArraysDict =
            new Dictionary<FieldAttributeKey, object>();

        public static IReadOnlyList<Type> GetParameterTypes(this MethodBase method)
        {
            return ParameterMap.GetOrAdd(method, c => (from p in c.GetParameters()
                select p.ParameterType).ToArray());
        }


        public static Func<object[], object> GetStaticFuncMethod(this Type type, string methodName,
            params Type[] paramTypes)
        {
            return type.GetStaticFuncMethod<object>(methodName, paramTypes);
        }

        public static Func<object, object> GetStaticFuncMethod(this Type type, string methodName, Type paramType)
        {
            return type.GetStaticFuncMethod<object>(methodName, paramType);
        }

        public static Func<object> GetStaticFuncMethod(this Type type, string methodName)
        {
            return type.GetStaticFuncMethod<object>(methodName);
        }

        public static Func<object[], TReturn> GetStaticFuncMethod<TReturn>(this Type type, string methodName,
            params Type[] paramTypes)
        {
            return type.GetMethodInfo(methodName, paramTypes)?
                .CreateDelegate<Func<object[], TReturn>>(true);
        }

        public static Func<object, TReturn> GetStaticFuncMethod<TReturn>(this Type type, string methodName,
            Type paramType)
        {
            return type.GetMethodInfo(methodName, paramType)?
                .CreateDelegate<Func<object, TReturn>>(true);
        }

        public static Func<TReturn> GetStaticFuncMethod<TReturn>(this Type type, string methodName)
        {
            return type.GetMethodInfo(methodName)?.CreateDelegate<Func<TReturn>>(true);
        }

        public static Func<TParam, TReturn> GetStaticFuncMethod<TParam, TReturn>(this Type type, string methodName)
        {
            return GetMethodInfo(type, methodName, typeof(TParam))?
                .CreateDelegate<Func<TParam, TReturn>>(true);
        }

        public static Func<TParam1, TParam2, TReturn> GetStaticFuncMethod<TParam1, TParam2, TReturn>(this Type type,
            string methodName)
        {
            return type.GetMethodInfo(methodName, typeof(TParam1), typeof(TParam2))?
                .CreateDelegate<Func<TParam1, TParam2, TReturn>>(true);
        }

        public static bool HasClassAttribute<T>(this Type type) where T : Attribute
        {
            return hasClassAttributeDict.GetOrAdd(new AttributeKey(type, typeof(T)), delegate(AttributeKey key)
            {
                T[] array = key.OwningType.GetCustomAttributes<T>().ToArray();
                if (array.Length == 0) return false;

                classAttributesDict.Add(key, array);
                return true;
            });
        }

        /// <summary>
        /// Returns First Attribute if Class has multiple
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attribute"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasClassAttribute<T>(this Type type, out T attribute) where T : Attribute
        {
            if (type.HasClassAttribute<T>())
            {
                attribute = ((T[]) classAttributesDict[new AttributeKey(type, typeof(T))])[0];
                return true;
            }

            attribute = default;
            return false;
        }

        /// <summary>
        /// don't know my intention here anymore...
        /// executing a method that handles a desired array of attributes, if that exists.
        /// but meh, whatever.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void PassClassAttributes<T>(this Type type, Action<T[]> action) where T : Attribute
        {
            T[] array = type.GetClassAttributes<T>().ToArray();
            if (array.Length != 0) action(array);
        }

        public static T[] GetClassAttributes<T>(this Type type) where T : Attribute
        {
            return (T[]) classAttributesDict.GetOrAdd(new AttributeKey(type, typeof(T)),
                key => key.OwningType.GetCustomAttributes<T>().ToArray());
        }

        public static bool HasFieldAttributes<T>(this Type parentType, string fieldName) where T : Attribute
        {
            return hasFieldAttributeDict.GetOrAdd(new FieldAttributeKey(parentType, fieldName, typeof(T)),
                key => key.OwningType.GetFieldAttributes<T>(key.FieldName).Any());
        }

        public static void PassFieldAttributes<T>(this Type parentType, string fieldName, Action<T[]> action)
            where T : Attribute
        {
            T[] fieldAttributes = parentType.GetFieldAttributes<T>(fieldName);
            if (fieldAttributes.Length != 0) action(fieldAttributes);
        }

        public static T[] GetFieldAttributes<T>(this Type parentType, string fieldName) where T : Attribute
        {
            object Function(FieldAttributeKey arg)
            {
                IEnumerable<Attribute> fieldAttributes = parentType.GetFieldAttributes(fieldName);
                return fieldAttributes.OfType<T>().ToArray();
            }

            return (T[]) fieldAttributeArraysDict.GetOrAdd(new FieldAttributeKey(parentType, fieldName, typeof(T)),
                Function);
        }

        public static Attribute[] GetFieldAttributes(this Type parentType, string fieldName)
        {
            return fieldAttributeDict.GetOrAdd(new MemberKey(parentType, fieldName),
                key => (Attribute[]) parentType.GetPropFieldRecursion(fieldName).GetCustomAttributes());
        }

        public static T CreateDelegate<T>(this MethodInfo mInfo, bool isStaticDelegate) where T : Delegate
        {
            Type delType = typeof(T);
            Type objectType = typeof(object);
            MethodInfo method = delType.GetMethod("Invoke");
            Type voidType = typeof(void);
            bool noValueReturn = false;
            if (mInfo.ReturnType == voidType)
            {
                noValueReturn = true;
                if (method.ReturnType != voidType)
                {
                    Debug.LogError("Requested Delegate " + delType.NiceName() +
                                   " does not return void");
                    return null;
                }
            }
            else if (method.ReturnType != typeof(object) &&
                     !method.ReturnType.IsSameSubOrConvertibleOf(mInfo.ReturnType))
            {
                Debug.LogError("Requested Delegate " + delType.NiceName() + " does not return " +
                               mInfo.ReturnType.NiceName());
                return null;
            }

            if (isStaticDelegate != mInfo.IsStatic)
            {
                Debug.LogError("Requested Delegate " + delType.NiceName() + " is " +
                               (isStaticDelegate ? "" : "not") + " static, but Method " + mInfo.Name +
                               "(" + string.Join(", ", mInfo.GetParameterTypes()) + ") is" +
                               (mInfo.IsStatic ? "" : " not") + ".");
                return null;
            }

            Type[] methodParamTypes = mInfo.GetParameterTypes().ToArray();
            Type[] delParamTypes = method.GetParameterTypes().ToArray();
            bool delParamsIsObjectArray = false;
            int delParamPos = !isStaticDelegate ? 1 : 0;
            if (methodParamTypes.Length == 0)
            {
                if (delParamTypes.Length != methodParamTypes.Length + delParamPos)
                {
                    Debug.LogError("Amount of Parameters of the requested Delegate " +
                                   delType.NiceName() + " doesn't fit the TargetMethod (" +
                                   mInfo.Name + ")'s parameters : (" +
                                   string.Join(", ", mInfo.GetParameterTypes()) + ")");
                    return null;
                }

                if (isStaticDelegate)
                    return Expression.Lambda<T>(Expression.Convert(Expression.Call(null, mInfo), method.ReturnType),
                        Array.Empty<ParameterExpression>()).Compile();

                if (delParamTypes.Length == 1)
                {
                    ParameterExpression parameterExpression = Expression.Parameter(delParamTypes[0]);
                    return Expression
                        .Lambda<T>(
                            Expression.Convert(
                                Expression.Call(Expression.Convert(parameterExpression, mInfo.DeclaringType), mInfo),
                                method.ReturnType), parameterExpression).Compile();
                }
            }

            if (delParamTypes[delParamPos].IsArray && delParamTypes[delParamPos].GetElementType() == typeof(object))
            {
                delParamsIsObjectArray = true;
            }
            else
            {
                if (methodParamTypes.Length != delParamTypes.Length - delParamPos)
                {
                    Debug.LogError("Amount of Parameters of the requested Delegate " +
                                   delType.NiceName() + " doesn't fit the TargetMethod (" +
                                   mInfo.Name + ")'s parameters : (" +
                                   string.Join(", ", mInfo.GetParameterTypes()) + ")");
                    return null;
                }

                if (!isStaticDelegate && delParamTypes[0] != objectType &&
                    !mInfo.DeclaringType.IsSameSubOrConvertibleOf(delParamTypes[0]))
                {
                    Debug.LogError("TargetObject of Delegate " + delType.NiceName() +
                                   " must be of Type " + $"{mInfo.DeclaringType} but is {delParamTypes[0]}");
                    return null;
                }
            }

            if (!delParamsIsObjectArray)
            {
                for (int i = delParamPos; i < delParamTypes.Length; i++)
                {
                    Type type = delParamTypes[i];
                    Type type2 = methodParamTypes[i - delParamPos];
                    if (type != objectType && !type2.IsSameSubOrConvertibleOf(type))
                    {
                        Debug.LogError(
                            $"DelegateParameter {i} ({type.NiceName()}) of Delegate {delType.NiceName()} can not be cast to " +
                            "MethodParameter " + type2.NiceName());
                        return null;
                    }
                }
            }

            ParameterExpression[] delParams = delParamTypes.Select(Expression.Parameter).ToArray();
            Expression[] delParamConversions = new Expression[delParams.Length + (delParamsIsObjectArray ? methodParamTypes.Length -1 : 0)];
            if (!isStaticDelegate)
            {
                delParamConversions[0] = Expression.Convert(delParams[0], mInfo.DeclaringType);
            }

            if (delParamsIsObjectArray)
            {
                MethodInfo convertAllMethod = typeof(Array).GetMethod(nameof(Array.ConvertAll));
                MethodInfo arrayConvertMethod = typeof(QuickAccess).GetMethod(nameof(ArrayCastConverter));

                ParameterExpression array5 = delParams[delParamPos];
                for (int j = 0; j < methodParamTypes.Length; j++)
                {
                    ConstantExpression index = Expression.Constant(j, typeof(int));
                    BinaryExpression expression = Expression.ArrayIndex(array5, index);
                    delParamConversions[delParamPos + j] = methodParamTypes[j].IsArray 
                        ? (Expression) Expression.Call(convertAllMethod.MakeGenericMethod(typeof(object), methodParamTypes[j].GetElementType()), Expression.Convert(expression, typeof(object[])), 
                            Expression.Call(arrayConvertMethod.MakeGenericMethod(methodParamTypes[j].GetElementType()))) 
                        : Expression.Convert(expression, methodParamTypes[j]);
                }
            }
            else
            {
                int metParamPos = 0;
                for (int k = delParamPos; k < delParamTypes.Length; k++)
                {
                    ParameterExpression paramExpr = delParams[k];
                    delParamConversions[k] = Expression.Convert(paramExpr, methodParamTypes[metParamPos]);
                    metParamPos++;
                }
            }

            Expression expression3 = Expression.Call(!isStaticDelegate ? delParamConversions[0] : null, mInfo, delParamConversions.Skip(delParamPos));
            if (!noValueReturn) expression3 = Expression.Convert(expression3, method.ReturnType);

            return Expression.Lambda<T>(expression3, delParams).Compile();
        }

        public static Converter<object, T> ArrayCastConverter<T>() => input => (T) input;

        public static bool IsSameSubOrConvertibleOf(this Type toType, Type fromType)
        {
            if (toType == fromType || toType.IsSubclassOf(fromType)) return true;

            Type[] array = new Type[2]
            {
                toType,
                fromType
            };
            string[] source = new string[2]
            {
                "op_Implicit",
                "op_Explicit"
            };
            for (int i = 0; i < array.Length; i++)
            {
                Type type = array[i];
                Type right = array[1 - i];
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (MethodInfo methodInfo in methods)
                    if (source.Contains(methodInfo.Name))
                    {
                        Type parameterType = methodInfo.GetParameters()[0].ParameterType;
                        if (methodInfo.ReturnType == type && parameterType == right ||
                            methodInfo.ReturnType == right && parameterType == type)
                            return true;
                    }
            }

            return false;
        }

        public static bool EqualsAny<T>(this T target, params T[] others)
        {
            return others.Contains(target);
        }

        public static PropertyInfo PropertyInfo(this Type type, string propertyName)
        {
            PropertyInfo result;
            if ((object) (result = type.PropFieldInfo(propertyName) as PropertyInfo) == null)
                Debug.LogError($"Couldn't find Property {propertyName} of Type {type}");

            return result;
        }

        public static MethodInfo GetMethodInfo(this Type type, string methodName, params Type[] parameterTypes)
        {
            if (type == null) throw new ArgumentException("Type is null");

            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("methodName is null or empty");

            return (MethodInfo) specificMethods.GetOrAdd(new MethodKey(type, methodName, parameterTypes),
                key =>
                {
                    MethodInfo method = key.OwningType.GetMethod(key.MethodName,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.FlattenHierarchy, null, key.ParameterTypes, null);
                    if (method == null)
                        Debug.LogError($"{nameof(GetMethodInfo)}: Couldn't find Method {key.MethodName} " +
                                       $"of Type {key.OwningType}");
                    return method;
                });
        }

        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            FieldInfo result = (FieldInfo) type.PropFieldInfo(fieldName);
            if (result == null)
                Debug.LogError($"{nameof(GetFieldInfo)} Couldn't find Field {fieldName} of Type {type}");

            return result;
        }

        private static MemberInfo PropFieldInfo(this Type type, string memberName)
        {
            return memberInfos.GetOrAdd(new MemberKey(type, memberName),
                key => key.OwningType.GetPropFieldRecursion(key.MemberName));
        }

        private static MemberInfo GetPropFieldRecursion(this Type type, string memberName)
        {
            Type arg = type;
            type = type.GetArrayOrListItemType();
            MemberInfo memberInfo = null;
            MemberInfo[] infos;
            do
            {
                infos = type.GetMember(memberName,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy);
                if (infos.Length == 0)
                {
                    type = RedirectedBaseType(type);
                }
                else
                {
                    memberInfo = infos[0];
                }
            }
            while (infos.Length == 0 && type != null);

            if (memberInfo == null)
            {
                Debug.LogError($"Couldn't find Property or Field {memberName} of Type {arg}");
            }

            return memberInfo;
        }

        private static Type RedirectedBaseType(Type type)
        {
            // if (type.HasInterfaceDefinition(typeof(IRedirectReflectionTo<>), out Type[] genericArguments))
            // {
            //     if (genericArguments[0] == type || genericArguments[0].IsSubclassOf(type))
            //     {
            //         Debug.LogError(
            //             $"{nameof(IRedirectReflectionTo<object>)}: Redirection targets {type} itself " +
            //             $"or a subclass of it, provoking an endless loop");
            //         return null;
            //     }
            //
            //     return genericArguments[0];
            // }

            return type.BaseType;
        }

        public static bool IsStaticMember(this MemberInfo info)
        {
            if ((object) info != null)
            {
                FieldInfo fieldInfo;
                if ((object) (fieldInfo = info as FieldInfo) != null)
                {
                    FieldInfo fieldInfo2 = fieldInfo;
                    return fieldInfo2.IsStatic;
                }

                MethodBase methodBase;
                if ((object) (methodBase = info as MethodBase) != null)
                {
                    MethodBase methodBase2 = methodBase;
                    return methodBase2.IsStatic;
                }

                PropertyInfo propertyInfo;
                if ((object) (propertyInfo = info as PropertyInfo) != null)
                {
                    PropertyInfo propertyInfo2 = propertyInfo;
                    return propertyInfo2.GetAccessors(true).Any(mInfo => mInfo.IsStatic);
                }
            }

            throw new NotSupportedException("InfoType is not supported");
        }

        public static Type[] GetInterfaces(this Type type)
        {
            if (!interfaceDict.TryGetValue(type, out Type[] value))
            {
                value = type.GetInterfaces();
                interfaceDict.Add(type, value);
            }

            return value;
        }

        public static bool HasInterface(this Type type, Type @interface)
        {
            if (!@interface.IsInterface) return false;

            InterfaceKey key = new InterfaceKey(type, @interface);
            if (!hasInterfaceDict.TryGetValue(key, out bool value))
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces.Contains(@interface))
                {
                    hasInterfaceDict.Add(key, true);
                    return true;
                }

                hasInterfaceDict.Add(key, false);
                return false;
            }

            return value;
        }

        public static bool HasInterfaceDefinition(this Type type, Type @interface)
        {
            if (!@interface.IsInterface) return false;

            InterfaceKey key = new InterfaceKey(type, @interface);
            return GetHIDValue(key).has;
        }

        public static bool HasInterfaceDefinition(this Type type, Type @interface, out Type[] genericArguments)
        {
            if (!@interface.IsInterface)
            {
                genericArguments = Array.Empty<Type>();
                return false;
            }

            InterfaceKey key = new InterfaceKey(type, @interface);
            InterfaceDefinition hIDValue = GetHIDValue(key);
            genericArguments = hIDValue.genericArguments;
            return hIDValue.has;
        }

        private static InterfaceDefinition GetHIDValue(InterfaceKey key)
        {
            if (!hasInterfaceDefDict.TryGetValue(key, out InterfaceDefinition value))
            {
                Type[] interfaces = GetInterfaces(key.owningType);
                bool flag = false;
                Type[] array = interfaces;
                foreach (Type type in array)
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == key.interfaceType)
                    {
                        value = new InterfaceDefinition(true, type.GetGenericArguments());
                        hasInterfaceDefDict.Add(key, value);
                        flag = true;
                        break;
                    }

                if (!flag)
                {
                    value = new InterfaceDefinition(false, Array.Empty<Type>());
                    hasInterfaceDefDict.Add(key, value);
                }
            }

            return value;
        }

        private static Delegate GetPropFieldGetter<TOwner, TReturn>(this Type type, string propertyName)
        {
            MemberInfo memberInfo = type.PropFieldInfo(propertyName);
            if (memberInfo == null)
            {
                return null;
            }

            return propFieldDelegates.GetOrAdd(new PropFieldKey(memberInfo, memberInfo.IsStaticMember(), false),
                BuildPropFieldGetter<TOwner, TReturn>);
        }

        private static Delegate BuildPropFieldGetter<TOwner, TReturn>(PropFieldKey key)
        {
            Type returnType = typeof(TReturn);
            Type objectType = typeof(object);
            Type ownerType = typeof(TOwner);
            if (returnType != objectType && returnType != key.PropFieldType &&
                !key.PropFieldType.IsSubclassOf(returnType))
            {
                Debug.LogError(
                    $"Property/FieldType {key.PropFieldType} is not of requested ReturnType {returnType}");
                return null;
            }

            bool ownerNeedsConversion = ownerType != key.MemberInfo.DeclaringType;
            bool returnNeedsConversion = ownerType != key.PropFieldType;

            ParameterExpression ownerParam = Expression.Parameter(ownerType);
            UnaryExpression ownerConversion = Expression.Convert(ownerParam, key.MemberInfo.DeclaringType);
            FieldInfo field;
            Expression expression = (object) (field = key.MemberInfo as FieldInfo) != null
                ? Expression.Field(key.IsStatic
                    ? null
                    : ownerNeedsConversion
                        ? (Expression) ownerConversion
                        : ownerParam, field)
                : (Expression) Expression.Call(key.IsStatic
                        ? null
                        : ownerConversion,
                    ((PropertyInfo) key.MemberInfo).GetMethod);
            if (returnNeedsConversion) // this might cause an Error
            {
                expression = Expression.Convert(expression, returnType);
            }

            return key.IsStatic
                ? Expression.Lambda(expression).Compile()
                : Expression.Lambda(expression, ownerParam).Compile();
        }

        private static Delegate GetPropFieldSetter<TOwner, TValue>(this Type type, string propertyName)
        {
            MemberInfo memberInfo = type.PropFieldInfo(propertyName);
            if (memberInfo == null)
            {
                return null;
            }

            return propFieldDelegates.GetOrAdd(new PropFieldKey(memberInfo, memberInfo.IsStaticMember(), true),
                BuildPropFieldSetter<TOwner, TValue>);
        }

        private static Delegate BuildPropFieldSetter<TOwner, TValue>(PropFieldKey key)
        {
            if (key.MemberInfo is FieldInfo finfo && finfo.IsInitOnly)
            {
                return null;
            }
            Type actualValueType = key.PropFieldType;
            Type tValueType = typeof(TValue);
            Type objectType = typeof(object);
            Type ownerType = typeof(TOwner);

            if (tValueType == objectType || tValueType == actualValueType ||
                actualValueType.IsSubclassOf(tValueType))
            {
                MemberInfo memberInfo = key.MemberInfo;
                ParameterExpression tValueTypeParam = Expression.Parameter(tValueType);
                UnaryExpression valueConversion = Expression.Convert(tValueTypeParam, actualValueType);
                bool needsValueConversion = tValueType != actualValueType;
                try
                {
                    if (!key.IsStatic)
                    {
                        ParameterExpression ownerParameter = Expression.Parameter(ownerType);
                        UnaryExpression ownerConversion =
                            Expression.Convert(ownerParameter, key.MemberInfo.ReflectedType); // can also be declaringType
                        bool ownerNeedsConversion = ownerType != key.MemberInfo.ReflectedType;

                        Expression body = key.MemberInfo is FieldInfo field
                            ? Expression.Assign(
                                Expression.Field(
                                    ownerNeedsConversion
                                        ? (Expression) ownerConversion
                                        : ownerParameter,
                                    field),
                                needsValueConversion
                                    ? (Expression) valueConversion
                                    : tValueTypeParam)
                            : (Expression) Expression.Call(
                                ownerNeedsConversion
                                    ? (Expression) ownerConversion
                                    : ownerParameter, ((PropertyInfo) memberInfo).SetMethod,
                                needsValueConversion
                                    ? (Expression) valueConversion
                                    : tValueTypeParam);
                        return Expression.Lambda(typeof(Action<TOwner, TValue>), body, ownerParameter,
                            tValueTypeParam).Compile();
                    }

                    Expression body2 = memberInfo is FieldInfo field2
                        ? Expression.Assign(Expression.Field(null, field2),
                            needsValueConversion
                                ? (Expression) valueConversion
                                : tValueTypeParam)
                        : (Expression) Expression.Call(((PropertyInfo) memberInfo).SetMethod, needsValueConversion
                            ? (Expression) valueConversion
                            : tValueTypeParam);
                    return Expression.Lambda(typeof(Action<TValue>), body2, tValueTypeParam).Compile();
                }
                catch (ArgumentNullException)
                {
                    PropertyInfo propertyInfo;
                    if ((object) (propertyInfo = memberInfo as PropertyInfo) != null && !propertyInfo.CanWrite)
                    {
                        Debug.LogError(
                            $"Property {key.MemberInfo.DeclaringType}.{key.MemberInfo.Name} is readonly");
                        return null;
                    }

                    throw;
                }
            }

            Debug.LogError($"Given ValueType {tValueType} is not of Type {key.PropFieldType}");
            return null;
        }

        // public static Func<object, object> PropFieldGetter(this Type type, string propFieldName)
        // {
        //     return type.PropFieldGetter<object>(propFieldName);
        // }

        public static Func<TOwner, TReturn> PropFieldGetter<TOwner, TReturn>(this Type type, string propFieldName)
        {
            return (Func<TOwner, TReturn>) type.GetPropFieldGetter<TOwner, TReturn>(propFieldName);
        }

        public static Action<TOwner, TValue> PropFieldSetter<TOwner, TValue>(this Type type, string propFieldName)
        {
            return (Action<TOwner, TValue>) type.GetPropFieldSetter<TOwner, TValue>(propFieldName);
        }

        // public static Func<object, T> PropFieldGetter<T>(this Type type, string propFieldName)
        // {
        //     return (Func<object, T>) type.GetPropFieldGetter<T>(propFieldName);
        // }
        //
        // public static Action<object, object> PropFieldSetter(this Type type, string propFieldName)
        // {
        //     return (Action<object, object>) type.GetPropFieldSetter<object>(propFieldName);
        // }

        // public static Action<object, T> PropFieldSetter<T>(this Type type, string propFieldName)
        // {
        //     return (Action<object, T>) type.GetPropFieldSetter<T>(propFieldName);
        // }

        public static PropField<TOwner, TValue> PropField<TOwner, TValue>(this Type type, string propFieldName)
        {
            return (PropField<TOwner, TValue>) propFields.GetOrAdd(new MemberKey(type, propFieldName),
                k => new PropField<TOwner, TValue>(k.OwningType, k.MemberName));
        }

        // public static PropField<TValue> PropField<TValue>(this Type type, string propFieldName)
        // {
        //     return (PropField<TValue>) propFields.GetOrAdd(new MemberKey(type, propFieldName),
        //         k => new PropField<TValue>(k.OwningType, k.MemberName));
        // }

        // public static PropField PropField(this Type type, string propFieldName)
        // {
        //     return (PropField) propFields.GetOrAdd(new MemberKey(type, propFieldName),
        //         k => new PropField(k.OwningType, k.MemberName));
        // }

        public static AssignedPropField<TOwner, TValue> AssignedPropField<TOwner, TValue>(this Type type,
            string propFieldName, TOwner targetObject)
        {
            return (AssignedPropField<TOwner, TValue>) assignedPropFields.GetOrAdd(
                new AssignedPropFieldKey(type, propFieldName, targetObject),
                k => new AssignedPropField<TOwner, TValue>(k.OwningType, k.FieldName, (TOwner) k.AssignedObject));
        }

        public static Func<TValue> StaticPropFieldGetter<TValue>(this Type type, string fieldName)
        {
            return (Func<TValue>) type.GetPropFieldGetter<object, TValue>(fieldName);
        }

        public static Action<TValue> StaticPropFieldSetter<TValue>(this Type type, string fieldName)
        {
            return (Action<TValue>) type.GetPropFieldSetter<object, TValue>(fieldName);
        }

        public static StaticPropField<TValue> StaticPropField<TValue>(this Type type, string fieldName)
        {
            return (StaticPropField<TValue>) propFields.GetOrAdd(new MemberKey(type, fieldName),
                k => new StaticPropField<TValue>(k.OwningType, k.MemberName));
        }
    }
}