using Datagrid.Net.Enums;

namespace Datagrid.Net.Structures
{
    public interface ISorterSettings
    {
        string ColumnName { get; }
        SortDirection SortDirection { get; }

        void Set(string columnName, SortDirection sortDirection);
    }

    public class SorterSettings : ISorterSettings
    {
        public virtual string ColumnName { get; private set; }
        public virtual SortDirection SortDirection { get; private set; }

        public virtual void Set(string columnName, SortDirection sortDirection)
        {
            ColumnName = columnName;
            SortDirection = sortDirection;
        }
    }
}