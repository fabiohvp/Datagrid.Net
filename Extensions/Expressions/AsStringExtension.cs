namespace System.Linq.Expressions
{
    public static class AsStringExtension
    {
        /// <summary>
        /// Default date and time format MM-DD-YYYY HH:mm:sss
        /// </summary>
        private static Expression<Func<DateTime, string>> DateTimeFormat = date => date.ToString();

        private static Expression _AsString(this Expression expression)
        {
            var stringConvert = typeof(object).GetMethod(nameof(object.ToString));
            return expression = Expression.Call(expression, stringConvert);
        }

        public static Expression AsString(this Expression expression)
        {
            if (expression.Type != typeof(string))
            {
                if (expression.Type == typeof(DateTime))
                {
                    expression = expression.AsDateTimeNullable();
                    expression = expression.Coalesce(() => expression.AsDateTimeString());
                }
                else if (expression.Type == typeof(DateTime?))
                {
                    expression = expression.Coalesce(() => expression.AsDateTimeString());
                }
                else
                {
                    expression = expression.Coalesce(() => expression._AsString());
                }
            }
            else
            {
                expression = expression.Coalesce(() => expression);
            }

            return expression;
        }

        private static Expression Coalesce(this Expression expression, Func<Expression> defaultAction)
        {
            var nullableType = Nullable.GetUnderlyingType(expression.Type);

            if (expression.Type == typeof(string) == true || nullableType != default(Type))
            {
                return Expression.Condition
                (
                    Expression.Equal(expression, Expression.Constant(default(object))),
                    Expression.Constant(string.Empty),
                    defaultAction.Invoke()
                );
            }

            return defaultAction.Invoke();
        }

        public static Expression AsDateTimeString(this Expression expression)
        {
            if (expression.Type == typeof(DateTime?))
            {
                var propertyInfo = expression.Type.GetProperty("Value");
                expression = Expression.Property(expression, propertyInfo);
            }

            return InternalExpressionExtensions.ReplaceParameter(DateTimeFormat, expression);
        }

        private static Expression AsDateTimeNullable(this Expression expression)
        {
            return Expression.Convert(expression, typeof(DateTime?));
        }
    }
}
