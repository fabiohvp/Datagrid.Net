using System.Collections.Generic;

namespace Datagrid.Net.Structures
{
    public interface IParameterSettings
    {
        IDictionary<string, string> Parameters { get; }
        List<string> Columns { get; }
        List<IFilterSettings> FilterSettings { get; }
        List<ISorterSettings> SorterSettings { get; }

        int PageNumber { get; }
        int PageSize { get; }
        bool Refresh { get; set; }

        void Set
        (
            IDictionary<string, string> parameters,
            List<string> columns,
            List<IFilterSettings> filterSettings,
            List<ISorterSettings> sorterSettings,
            int pageNumber,
            int pageSize,
            bool refresh
        );
    }

    public class ParameterSettings : IParameterSettings
    {
        private int _PageNumber;

        public virtual List<string> Columns { get; private set; }
        public virtual int PageNumber
        {
            get { return _PageNumber; }
            private set
            {
                _PageNumber = value;

                if (value < 0)
                {
                    _PageNumber = 0;
                }
            }
        }
        public virtual int PageSize { get; private set; }
        public virtual IDictionary<string, string> Parameters { get; private set; }

        public virtual List<IFilterSettings> FilterSettings { get; private set; }
        public virtual List<ISorterSettings> SorterSettings { get; private set; }

        public virtual bool Refresh { get; set; }

        public ParameterSettings()
        { }

        public void Set
        (
            IDictionary<string, string> parameters,
            List<string> columns,
            List<IFilterSettings> filterSettings,
            List<ISorterSettings> sorterSettings,
            int pageNumber,
            int pageSize,
            bool refresh
        )
        {
            Columns = columns;
            FilterSettings = filterSettings;
            SorterSettings = sorterSettings;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Parameters = parameters;
            Refresh = refresh;
        }
    }
}
