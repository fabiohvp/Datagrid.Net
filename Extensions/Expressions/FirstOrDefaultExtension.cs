using System.Reflection;

namespace System.Linq.Expressions
{
    public static class FirstOrDefaultExtension
    {
        public static Expression FirstOrDefault(this Expression expression, Type sourceType)
        {
            var firstOrDefaultMethod = typeof(Enumerable).GetMethods()
                .FirstOrDefault(o => o.Name == "FirstOrDefault" && o.GetParameters().Length == 1)
                .MakeGenericMethod(new Type[] { sourceType });

            return Expression.Call(firstOrDefaultMethod, expression);
        }

        public static Expression FirstOrDefault(this Expression expression, Type propertyType, MethodInfo comparerMethod, string keyword)
        {
            if (string.IsNullOrEmpty(keyword) == false)
            {
                keyword = keyword.ToLower();
            }

            var firstOrDefaultMethod = typeof(Enumerable).GetMethods()
                .First(o => o.Name == nameof(Enumerable.FirstOrDefault) && o.GetParameters().Length == 2);

            var firstOrDefaultDelegateType = typeof(Func<,>).MakeGenericType(propertyType, typeof(bool));
            var firstOrDefaultPredicateParameter = Expression.Parameter(propertyType);

            //o => o.ComparerMethod(keyword)
            var firstOrDefaultCompareExpression = Expression.Call
            (
                firstOrDefaultPredicateParameter.AsString().ToLower(),
                comparerMethod,
                Expression.Constant(keyword, typeof(string))
            );

            //expression.FirstOrDefault(firstOrDefaultCompareExpression);
            return Expression.Call
            (
                firstOrDefaultMethod.MakeGenericMethod(new Type[] { propertyType }),
                expression,
                Expression.Lambda(firstOrDefaultDelegateType, firstOrDefaultCompareExpression, firstOrDefaultPredicateParameter)
            );
        }
    }
}
