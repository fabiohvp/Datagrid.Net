
namespace System.Linq.Expressions
{
    public static class ToLowerExtension
    {
        public static Expression ToLower(this Expression expression)
        {
            return Expression.Call(expression, typeof(string).GetMethod(nameof(String.ToLower), new Type[] { }));
        }
    }
}
