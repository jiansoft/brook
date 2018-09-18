using System;

namespace jIAnSoft.Brook.Utility
{
    internal static class Conversion
    {
        public static object ConvertTo<T>(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case T _:
                    return value;
            }
            if (typeof(T).IsPrimitive())
            {
                return Convert.ChangeType(value, typeof(T));
            }
            if (!typeof(T).IsConstructedGenericType())
            {
                return value;
            }
            if (!typeof(T).IsNullable())
            {
                return value;
            }
            var type = typeof(T).GetGenericTypeArguments()[0];
            return type.IsPrimitive() ? Convert.ChangeType(value, type) : value;
        }
    }
}
