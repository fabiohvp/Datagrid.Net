using System.Reflection;

namespace System
{
    public static class PropertyExtension
    {
        public static PropertyInfo GetProperty(this Type type, string name, bool caseSensitive = true)
        {
            if (caseSensitive)
            {
                return type.GetProperty(name);
            }

            return type.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }
    }
}
