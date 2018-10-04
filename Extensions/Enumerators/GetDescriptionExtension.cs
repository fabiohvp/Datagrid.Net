using System.ComponentModel;

namespace System
{
    public static class GetDescriptionExtension
    {
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = (DescriptionAttribute[])memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), true);

            return (attributes.Length > 0) ? attributes[0].Description : null;
        }
    }
}
