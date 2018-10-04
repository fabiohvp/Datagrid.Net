using System.Reflection;

namespace System.Linq.Expressions
{
    public static class CreateLambdaExtension
    {
        public static Expression<Func<T, bool>> CreateComparerLambda<T>(this string propertyName, string compareMethodName, string keyword)
        {
            var type = typeof(T);
            var parameter = Expression.Parameter(type);
            var expression = (Expression)parameter;

            compareMethodName = compareMethodName.ToLower();

            var comparerMethod = typeof(string).GetMethods()
                .First
                (m =>
                    m.Name.ToLower() == compareMethodName
                    && m.GetParameters().Length == 1
                    && m.ReturnType == typeof(bool)
                );

            expression = expression.GetPropertyExpression(propertyName, ref type, comparerMethod, keyword)
                .AsString()
                .ToLower();

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return CreateDateTimeComparerLambda<T>(expression, parameter, keyword);
            }

            var compareExpression = Expression.Call(expression, comparerMethod, Expression.Constant(keyword, typeof(string)));

            //o => o.ComparerMethod((property ?? "").ToLower() == keyword)
            return Expression.Lambda<Func<T, bool>>(compareExpression, parameter);
        }

        private static Expression<Func<T, bool>> CreateDateTimeComparerLambda<T>(Expression expression, ParameterExpression parameter, string keyword)
        {
            var indexOfEmptySpace = Expression.Call(expression, InternalExpressionExtensions.GetIndexOfMethod(), Expression.Constant(" "));
            var dateExpression = Expression.Call(expression, InternalExpressionExtensions.GetSubstringWithStartAndEndMethod(), Expression.Constant(0), indexOfEmptySpace);
            var timeExpression = Expression.Call(expression, InternalExpressionExtensions.GetSubstringWithStartMethod(), Expression.Add(indexOfEmptySpace, Expression.Constant(1)));

            var date = keyword;
            var time = keyword;

            if (keyword.Contains(' ') == true)
            {
                var dateAndTime = keyword.Split(new char[] { ' ' });

                var dateComparerExpression = GetDateComparerExpression<T>(dateExpression, parameter, dateAndTime[0]);
                var timeComparerExpression = GetTimeComparerExpression<T>(timeExpression, parameter, dateAndTime[1], InternalExpressionExtensions.GetStartsWithMethod());

                //o => o.ComparerMethod((property ?? "").Substring(0, property.IndexOf(' ')).ToLower().Contains(date))
                //  .And(o.ComparerMethod((property ?? "").Substring(property.IndexOf(' ')).ToLower().StartsWith(time)))
                return dateComparerExpression.And(timeComparerExpression);
            }
            else
            {
                var dateComparerExpression = GetDateComparerExpression<T>(dateExpression, parameter, date);
                var timeComparerExpression = GetTimeComparerExpression<T>(timeExpression, parameter, time, InternalExpressionExtensions.GetContainsMethod());

                //o => o.ComparerMethod((property ?? "").Substring(0, property.IndexOf(' ')).ToLower().Contains(date))
                //  .Or(o.ComparerMethod((property ?? "").Substring(property.IndexOf(' ')).ToLower().Contains(time)))
                return dateComparerExpression.Or(timeComparerExpression);
            }
        }

        private static Expression<Func<T, bool>> GetDateComparerExpression<T>(Expression dateExpression, ParameterExpression parameter, string date)
        {
            var compareDateExpression = Expression.Call(dateExpression, InternalExpressionExtensions.GetContainsMethod(), Expression.Constant(date, typeof(string)));

            return Expression.Lambda<Func<T, bool>>(compareDateExpression, parameter);
        }

        private static Expression<Func<T, bool>> GetTimeComparerExpression<T>(Expression timeExpression, ParameterExpression parameter, string time, MethodInfo comparerMethod)
        {
            var compareTimeExpression = Expression.Call(timeExpression, comparerMethod, Expression.Constant(time, typeof(string)));

            return Expression.Lambda<Func<T, bool>>(compareTimeExpression, parameter);
        }
    }
}