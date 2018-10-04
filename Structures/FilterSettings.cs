using Datagrid.Net.Enums;

namespace Datagrid.Net.Structures
{
    public interface IFilterSettings
    {
        string ColumnName { get; }
        string Text { get; }
        Operand Operand { get; }
        string CompareMethod { get; }

        void Set(string columnName, string text, Operand operand, string compareMethod);
    }

    public class FilterSettings : IFilterSettings
    {
        public virtual string ColumnName { get; private set; }
        public virtual string Text { get; private set; }
        public virtual Operand Operand { get; private set; }
        public virtual string CompareMethod { get; private set; }

        public void Set(string columnName, string text, Operand operand, string compareMethod)
        {
            ColumnName = columnName;
            Text = text;
            Operand = operand;
            CompareMethod = compareMethod;
        }
    }
}
