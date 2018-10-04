using System.Linq.Expressions;

namespace System.Linq
{
    public static class OrderExtension
    {
        //http://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet
        public static IOrderedQueryable<T> Order<T>(this IQueryable<T> source, string propertyName, string methodName)
        {
            var type = typeof(T);
            var parameter = Expression.Parameter(type);
            var expression = (Expression)parameter;

            expression = expression.GetPropertyExpression(propertyName, ref type);

            var orderDelegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var orderLambda = Expression.Lambda(orderDelegateType, expression, parameter);

            var comparerMethod = typeof(Queryable).GetMethods().Single
                (method =>
                    method.Name == methodName
                    && method.IsGenericMethodDefinition
                    && method.GetGenericArguments().Length == 2
                    && method.GetParameters().Length == 2
                )
                .MakeGenericMethod(typeof(T), type);

            return (IOrderedQueryable<T>)comparerMethod.Invoke(null, new object[] { source, orderLambda });
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return source.Order(property, nameof(Enumerable.OrderBy));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return source.Order(property, nameof(Enumerable.OrderByDescending));
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return source.Order(property, nameof(Enumerable.ThenBy));
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return source.Order(property, nameof(Enumerable.ThenByDescending));
        }
    }
}
