using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class AttributeExtension
    {
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type)
        {
            return type.GetCustomAttributes(typeof(TAttribute), true)
                .AsEnumerable()
                .Cast<TAttribute>();
        }

        public static TAttribute GetFirstAttribute<TAttribute>(this Type type)
        {
            return GetAttributes<TAttribute>(type).FirstOrDefault();
        }
    }
}
