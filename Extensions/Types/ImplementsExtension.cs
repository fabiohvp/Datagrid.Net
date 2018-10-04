namespace System
{
    public static class ImplementsExtension
    {
        public static bool ImplementsInterface(this Type type, Type @interface)
        {
            return type.GetInterface(@interface.Name) != default(Type);
        }
    }
}
