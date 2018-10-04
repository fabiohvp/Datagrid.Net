using Datagrid.Net.Attributes;
using Datagrid.Net.Enums;
using Datagrid.Net.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Datagrid.Net.Managers
{
    public interface ISorterManager : IManager
    {
        IList<ISorterSettings> CreateOrderAttributeColumnsSettingsCollection<TDatagridViewModel>(ISorterSettings sorterSettings);

        IOrderedQueryable<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, IEnumerable<ISorterSettings> sorterSettings);
    }

    public class SorterManager : ISorterManager
    {
        public virtual IDatagridSettings DatagridSettings { get; protected set; }

        public SorterManager(IDatagridSettings datagridSettings)
        {
            DatagridSettings = datagridSettings;
        }

        public virtual IList<ISorterSettings> CreateOrderAttributeColumnsSettingsCollection<TDatagridViewModel>(ISorterSettings sorterSettings)
        {
            var orderAttributes = (OrderByAttribute[])typeof(TDatagridViewModel).GetCustomAttributes(typeof(OrderByAttribute), true);
            var sorterSettingsCollection = new List<ISorterSettings>();

            if (orderAttributes.Length == 0)
            {
                sorterSettingsCollection.Add(sorterSettings);
            }
            else
            {
                foreach (var orderAttribute in orderAttributes)
                {
                    sorterSettings = DatagridSettings.Factory.CreateStructure<ISorterSettings>();

                    sorterSettings.Set(orderAttribute.ColumnName, orderAttribute.Direction);
                    sorterSettingsCollection.Add(sorterSettings);
                }
            }

            return sorterSettingsCollection;
        }

        public virtual IOrderedQueryable<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, IEnumerable<ISorterSettings> sorterSettingsCollection)
        {
            if (!sorterSettingsCollection.Any())
            {
                var queryTypeString = query.ToString().ToLower();

                //(order[\s]by([\[\].\s\w]+)(asc|desc)) = verify if query has been ordered already
                //(system.linq.orderedqueryable)|(system.linq.orderedenumerable) = when it is already in memory, verify by type name
                if (Regex.Match(queryTypeString, @"(order[\s]by([\[\].\s\w]+)(asc|desc))|(system.linq.orderedqueryable)|(system.linq.orderedenumerable)").Success)
                {
                    return query as IOrderedQueryable<TDatagridViewModel>;
                }

                var sorterSettings = DatagridSettings.Factory.CreateStructure<ISorterSettings>();

                sorterSettings.Set
                (
                    DatagridSettings.ParameterSettings.Columns.ElementAt(0),
                    SortDirection.Desc
                );

                sorterSettingsCollection = CreateOrderAttributeColumnsSettingsCollection<TDatagridViewModel>(sorterSettings);
            }

            var orderedQueryable = Process(query as IQueryable<TDatagridViewModel>, sorterSettingsCollection.FirstOrDefault());

            foreach (var sortColumnSettings in sorterSettingsCollection.Skip(1))
            {
                orderedQueryable = Process(orderedQueryable, sortColumnSettings);
            }

            return orderedQueryable;
        }

        protected virtual IOrderedQueryable<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> collection, ISorterSettings sorterSettings)
        {
            return sorterSettings.SortDirection == SortDirection.Asc
                ? collection.OrderBy(sorterSettings.ColumnName)
                : collection.OrderByDescending(sorterSettings.ColumnName);
        }

        protected virtual IOrderedQueryable<TDatagridViewModel> Process<TDatagridViewModel>(IOrderedQueryable<TDatagridViewModel> collection, ISorterSettings sorterSettings)
        {
            return sorterSettings.SortDirection == SortDirection.Asc
                ? collection.ThenBy(sorterSettings.ColumnName)
                : collection.ThenByDescending(sorterSettings.ColumnName);
        }
    }
}
