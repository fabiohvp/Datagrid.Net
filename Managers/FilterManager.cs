using Datagrid.Net.Enums;
using Datagrid.Net.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Datagrid.Net.Managers
{
    public interface IFilterManager : IManager
    {
        Expression<Func<TDatagridViewModel, bool>> ExecuteOperand<TDatagridViewModel>(Operand operand, Expression<Func<TDatagridViewModel, bool>> lambda, IEnumerable<IFilterSettings> filterSettingsCollection);

        IQueryable<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, IEnumerable<IFilterSettings> filterSettingsCollection);
    }

    public class FilterManager : IFilterManager
    {
        public virtual IDatagridSettings DatagridSettings { get; protected set; }

        public FilterManager(IDatagridSettings datagridSettings)
        {
            DatagridSettings = datagridSettings;
        }

        private static Expression<Func<TDatagridViewModel, bool>> ExecuteOperand<TDatagridViewModel>(Operand operand, Expression<Func<TDatagridViewModel, bool>> lambda, Expression<Func<TDatagridViewModel, bool>> newLambda)
        {
            if (operand == Operand.And)
            {
                return lambda.And(newLambda);
            }

            return lambda.Or(newLambda);
        }

        public virtual Expression<Func<TDatagridViewModel, bool>> ExecuteOperand<TDatagridViewModel>(Operand operand, Expression<Func<TDatagridViewModel, bool>> lambda, IEnumerable<IFilterSettings> filterSettingsCollection)
        {
            Func<
                Operand, //operand
                Expression<Func<TDatagridViewModel, bool>>, //lambda
                Expression<Func<TDatagridViewModel, bool>>, //new lambda
                Expression<Func<TDatagridViewModel, bool>>  //returns lambda.Operand(new lambda)
            > executeOperand = (_operand, _lambda, _newLambda) =>
            {
                if (_lambda == null)
                {
                    //executeOperand = (__operand, __lambda, __newLambda) =>
                    //{
                    //    return ExecuteOperand(__operand, __lambda, __newLambda);
                    //};

                    return _newLambda;
                }

                return ExecuteOperand(_operand, _lambda, _newLambda);
            };

            foreach (var filterSettings in filterSettingsCollection)
            {
                var translation = filterSettings.ColumnName.CreateComparerLambda<TDatagridViewModel>(
                    filterSettings.CompareMethod,
                    DatagridSettings.TranslatorManager.GetTranslation(filterSettings.ColumnName, filterSettings.Text)
                );

                lambda = executeOperand.Invoke
                (
                    filterSettings.Operand,
                    lambda,
                    translation
                );
            }

            return lambda;
        }

        public virtual IQueryable<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, IEnumerable<IFilterSettings> filterSettingsCollection)
        {
            Expression<Func<TDatagridViewModel, bool>> lambda = null;

            foreach (var filterSettings in filterSettingsCollection)
            {
                var newFilterSettingsCollection = new List<IFilterSettings>() { filterSettings };

                lambda = ExecuteOperand(filterSettings.Operand, lambda, newFilterSettingsCollection);
            }

            if (lambda != null)
            {
                return query.Where(lambda);
            }

            return query;
        }
    }
}
