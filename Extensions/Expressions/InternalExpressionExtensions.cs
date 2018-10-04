using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Linq.Expressions
{
    internal static class InternalExpressionExtensions
    {
        public static Expression GetPropertyExpression(this Expression expression, string propertyName, ref Type propertyType, MethodInfo comparerMethod = default(MethodInfo), string keyword = default(string))
        {
            var nestedPropertyNames = propertyName.Split('.').ToList();

            for (int i = 0; i < nestedPropertyNames.Count; i++)
            {
                var isNullableType = Nullable.GetUnderlyingType(propertyType);
                var nestedPropertyName = nestedPropertyNames[i];

                if (isNullableType != default(Type))
                {
                    nestedPropertyNames.Insert(0, "Value");
                    nestedPropertyName = nestedPropertyNames[i];
                }

                var propertyInfo = propertyType.GetProperty(nestedPropertyName, false);
                propertyType = propertyInfo.PropertyType;

                if (propertyInfo.PropertyType.GetGenericArguments().Any())
                {
                    propertyType = propertyInfo.PropertyType.GetGenericArguments().First();
                }

                if (propertyInfo.PropertyType.Is(typeof(string)) == false && propertyInfo.PropertyType.Is(typeof(IEnumerable)))
                {
                    expression = GetFirstOrDefaultProperty(expression, propertyInfo, ref propertyType, nestedPropertyNames[i + 1], comparerMethod, keyword);
                    i++;
                }
                else
                {
                    if (isNullableType == default(Type))
                    {
                        propertyType = propertyInfo.PropertyType;
                    }
                    else
                    {
                        propertyType = isNullableType;
                    }

                    expression = Expression.Property(expression, propertyInfo);
                }
            }

            return expression;
        }

        private static Expression GetFirstOrDefaultProperty(Expression propertyExpression, PropertyInfo propertyInfo, ref Type propertyType, string innerPropertyName, MethodInfo comparerMethod, string keyword)
        {
            propertyExpression = Expression.Property(propertyExpression, propertyInfo);

            if (propertyType.IsPrimitive == false && propertyType.Is(typeof(string)) == false)
            {

                propertyExpression = propertyExpression.Select(innerPropertyName, propertyType);
                propertyType = propertyExpression.Type.GetGenericArguments().First();
            }

            if (comparerMethod == default(MethodInfo) || string.IsNullOrEmpty(keyword) == true)
            {
                propertyExpression = propertyExpression.FirstOrDefault(propertyType);
            }
            else
            {
                propertyExpression = propertyExpression.FirstOrDefault(propertyType, comparerMethod, keyword);
            }

            return propertyExpression;
        }

        internal static MethodInfo GetContainsMethod()
        {
            return typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
        }

        internal static MethodInfo GetIndexOfMethod()
        {
            return typeof(string).GetMethod(nameof(string.IndexOf), new Type[] { typeof(string) });
        }

        internal static MethodInfo GetSubstringWithStartMethod()
        {
            return typeof(string).GetMethod(nameof(string.Substring), new Type[] { typeof(int) });
        }

        internal static MethodInfo GetSubstringWithStartAndEndMethod()
        {
            return typeof(string).GetMethod(nameof(string.Substring), new Type[] { typeof(int), typeof(int) });
        }

        internal static MethodInfo GetStartsWithMethod()
        {
            return typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) });
        }

        internal static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters
                         .Select((f, i) => new { f, s = second.Parameters[i] })
                         .ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from 
            // the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            // apply composition of lambda expression bodies to parameters from 
            // the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        internal class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> map;

            public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;

                if (map.TryGetValue(p, out replacement))
                {
                    p = replacement;
                }

                return base.VisitParameter(p);
            }
        }

        internal static Expression ReplaceParameter(this LambdaExpression expression, Expression target)
        {
            return new ParameterReplacer { Source = expression.Parameters[0], Target = target }.Visit(expression.Body);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            public ParameterExpression Source;
            public Expression Target;
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == Source ? Target : base.VisitParameter(node);
            }
        }
    }
}
