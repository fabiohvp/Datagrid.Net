namespace System
{
    public static class IsExtension
    {
        public static bool Is(this Type type, Type targetType)
        {
            return type == targetType || type.IsSubclassOf(targetType) || (targetType.IsInterface && type.ImplementsInterface(targetType));
        }
    }
}
