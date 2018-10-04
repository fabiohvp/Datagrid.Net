using Datagrid.Net.Enums;
using System;

namespace Datagrid.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OrderByAttribute : Attribute
    {
        public readonly string ColumnName;
        public readonly SortDirection Direction;

        public OrderByAttribute(string columnName, SortDirection direction)
        {
            ColumnName = columnName;
            Direction = direction;
        }
    }
}
