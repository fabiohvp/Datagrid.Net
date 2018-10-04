using System.Reflection;

namespace System.Linq.Expressions
{
    public static class SelectExtension
    {
        public static Expression Select(this Expression expression, string propertyName, Type resultType)
        {
            var property = resultType.GetProperty(propertyName, false);
            var selectMethod = GetSelectMethod();
            var selectDelegateType = typeof(Func<,>).MakeGenericType(resultType, property.PropertyType);
            var selectPredicateParameter = Expression.Parameter(resultType);
            var selectPredicateProperty = Expression.Property(selectPredicateParameter, property);

            return Expression.Call(
                selectMethod.MakeGenericMethod(new Type[] { resultType, property.PropertyType }),
                expression,
                Expression.Lambda(selectDelegateType, selectPredicateProperty, selectPredicateParameter)
            );
        }

        private static MethodInfo GetSelectMethod()
        {
            return typeof(Enumerable).GetMethods()
                .First
                (o =>
                    o.Name == nameof(Enumerable.Select)
                    && o.GetParameters().Length == 2
                );
        }
    }
}
