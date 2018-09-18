using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace jIAnSoft.Brook.Utility
{
    public static class ReflectionHelpers
    {
        internal static IEnumerable<FieldInfo> GetFields(this Type type)
        {
            return type.GetRuntimeFields();
        }
        internal static IEnumerable<PropertyInfo> GetProperties(this Type type)
        {
            return type.GetRuntimeProperties();
        }

        internal static MethodInfo GetMethod(this Type type, string name, Type[] parameters)
        {
            return type.GetRuntimeMethod(name, parameters);
        }

        internal static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        internal static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        internal static bool IsConstructedGenericType(this Type type)
        {
            return type.IsConstructedGenericType;
        }

        internal static IEnumerable<ConstructorInfo> GetConstructors(this Type type)
        {
            return
                type.GetTypeInfo()
                    .DeclaredConstructors.Where(
                        c => (c.Attributes & MethodAttributes.Static) == MethodAttributes.PrivateScope);
        }

        internal static Type[] GetGenericTypeArguments(this Type type)
        {
            return type.GenericTypeArguments;
        }

        /// <summary>
        /// This method helps simplify the process of getting a constructor for a type.
        ///             A method like this exists in .NET but is not allowed in a Portable Class Library,
        ///             so we've built our own.
        /// 
        /// </summary>
        /// <param name="self"/><param name="parameterTypes"/>
        /// <returns/>
        internal static ConstructorInfo FindConstructor(this Type self, params Type[] parameterTypes)
        {
            var constructors =
                from constructor in self.GetConstructors()
                let parameters = constructor.GetParameters()
                let types =
                parameters.Select(p => p.ParameterType)
                where types.SequenceEqual(parameterTypes)
                select constructor;
            return constructors.SingleOrDefault();
            //return
            //    GetConstructors(self).Select(constructor => new {
            //        constructor,
            //        parameters = constructor.GetParameters()
            //    }).Select(param0 => new {
            //        Eh__TransparentIdentifier2 = param0,
            //        types =
            //            param0.parameters.Select(p => p.ParameterType)
            //    })
            //        .Where(param0 => param0.types.SequenceEqual(parameterTypes))
            //        .Select(param0 => param0.Eh__TransparentIdentifier2.constructor)
            //        .SingleOrDefault();
        }

        internal static PropertyInfo GetProperty(this Type type, string name)
        {
            return type.GetRuntimeProperty(name);
        }

        internal static bool IsNullable(this Type t)
        {
            if (t.IsConstructedGenericType)
                return t.GetGenericTypeDefinition() == typeof(Nullable<>);
            return false;
        }


        private static void SetValue<T>(object obj, string key, object value)
        {
            var otp = obj.GetType();
            var field = otp.GetField(key,
                BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static);
            if (field != null)
            {
                field.SetValue(obj,
                    value == DBNull.Value
                        ? null
                        : value is T
                            ? (T)value
                            : value);
                return;
            }
           
            var property = otp.GetProperty(key,
                BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static);
           if (null == property) return;
            property?.SetValue(obj,
                    value == DBNull.Value
                        ? null
                        : value is T
                            ? (T) value
                            : value);
        }

        public static void SetValue(object obj, string key, object value)
        {
            SetValue<object>(obj, key, value);
        }

        private static IEnumerable<FieldInfo> GetFields(IReflect ot)
        {
            return ot.GetFields(BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public |
                                BindingFlags.Instance | BindingFlags.Static);
        }

        private static IEnumerable<PropertyInfo> GetProperties(IReflect ot)
        {
            return ot.GetProperties(BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public |
                                    BindingFlags.Instance | BindingFlags.Static);
        }

        /// <summary>
        /// 將DataTable內所有的的資料取出後放進 T 型別的物件內後回傳 T 型別陣列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static T[] ConvertAs<T>(DataTable dt)
        {
            return dt.Rows.Count <= 0
                ? new T[0]
                : (from DataRow dr in dt.Rows.AsParallel() select ConvertAs<T>(dr)).ToArray();
        }

        /// <summary>
        /// 將 DataRow 內的的資料取出後放進 T 型別的物件內後回傳。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T ConvertAs<T>(DataRow row)
        {
            var classobj = Activator.CreateInstance<T>();
            foreach (var column in row.Table.Columns)
            {
                SetValue(classobj, column.ToString(), row[column.ToString()]);
            }
            return classobj;
        }
        
        /// <summary>
        /// 將 object 內的的資料取出後放進 T 型別的物件內後回傳。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T ConvertAs<T>(object o)
        {
            var baseobj = Activator.CreateInstance<T>();
            foreach (var f in GetFields(o.GetType()))
            {
                SetValue(baseobj, f.Name, f.GetValue(o));
            }
            foreach (var p in GetProperties(o.GetType()))
            {
                SetValue(baseobj, p.Name, p.GetValue(o));
            }
            return baseobj;
        }
    }
}

