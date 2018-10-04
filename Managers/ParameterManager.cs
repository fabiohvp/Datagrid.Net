using Datagrid.Net.Enums;
using Datagrid.Net.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Datagrid.Net.Managers
{
    public interface IParameterManager : IManager
    {
        IParameterSettings Process(IDictionary<string, string> parameters);
    }

    public class ParameterManager : IParameterManager
    {
        private class Column
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public bool Orderable { get; set; }
            public bool Searchable { get; set; }
            public string SearchRegex { get; set; }
            public string SearchValue { get; set; }
            public IEnumerable<string> Associates { get; set; }
            public bool IsValueFormatted { get; set; }
        }

        private List<KeyValuePair<int, Column>> _Columns;
        private Dictionary<int, SortDirection> _Orders;

        public IDatagridSettings DatagridSettings { get; protected set; }

        public ParameterManager(IDatagridSettings datagridSettings)
        {
            DatagridSettings = datagridSettings;
        }

        public IParameterSettings Process(IDictionary<string, string> parameters)
        {
            _Columns = new List<KeyValuePair<int, Column>>();
            _Orders = new Dictionary<int, SortDirection>();

            var parameterSettings = DatagridSettings.Factory.CreateStructure<IParameterSettings>();
            var filterSettingsCollection = new List<IFilterSettings>();
            var sorterSettingsCollection = new List<ISorterSettings>();

            AddColumns(parameters);

            var searchValue = default(string);

            if (parameters.ContainsKey("search.value"))
            {
                searchValue = parameters["search.value"];
            }
            else if (parameters.ContainsKey("search[value]"))
            {
                searchValue = parameters["search[value]"];
            }

            AddGlobalSearchSettings(filterSettingsCollection, searchValue);
            AddFilterSettings(filterSettingsCollection);
            AddOrderSettings(sorterSettingsCollection);

            var start = 0;
            var length = 10;

            if (parameters.ContainsKey("start"))
            {
                int.TryParse(parameters["start"], out start);
            }
            else if (parameters.ContainsKey("length"))
            {
                int.TryParse(parameters["length"], out length);
            }

            start -= 1;
            start += length;
            start /= length;

            var refresh = false;

            parameterSettings.Set
            (
                parameters,
                _Columns.Select(o => o.Value.Data).ToList(),
                filterSettingsCollection,
                sorterSettingsCollection,
                start,
                length,
                refresh
            );

            return parameterSettings;
        }

        private void AddColumns(IDictionary<string, string> parameters)
        {
            var items = parameters.OrderBy(o => o.Key);

            var index = -1;
            var regexIndex = new Regex(@"(\[(\d)+\])");

            foreach (var item in items.Where(o => o.Key.StartsWith("columns") && (o.Key.EndsWith(".data") || o.Key.EndsWith("[data]"))))
            {
                var captures = regexIndex.Match(item.Key).Captures;

                if (captures.Count > 0)
                {
                    int.TryParse(captures[0].Value.Trim(new char[] { '[', ']' }), out index);

                    if (index > -1)
                    {
                        var column = GetColumn(items, index);

                        _Columns.Add(new KeyValuePair<int, Column>(index, column));
                        index = -1;
                    }
                }
            }

            foreach (var item in items.Where(o => o.Key.StartsWith("order")))
            {
                if (item.Key.EndsWith(".column") || item.Key.EndsWith("[column]"))
                {
                    index = -1;
                    int.TryParse(item.Value, out index);

                    if (index > -1)
                    {
                        if (_Orders.ContainsKey(index) == false)
                        {
                            _Orders.Add(index, SortDirection.Asc);
                        }
                    }
                }
                else if (item.Key.EndsWith(".dir") || item.Key.EndsWith("[dir]"))
                {
                    if (item.Value == "desc")
                    {
                        _Orders[index] = SortDirection.Desc;
                    }
                    else
                    {
                        _Orders[index] = SortDirection.Asc;
                    }
                }
            }
        }

        private void AddFilterSettings(List<IFilterSettings> filterSettingsCollection)
        {
            foreach (var column in _Columns)
            {
                var associates = GetAssociates(column);

                foreach (var item in associates)
                {
                    if (item.Value.Searchable && string.IsNullOrEmpty(column.Value.SearchValue) == false)
                    {
                        var filterSettings = DatagridSettings.Factory.CreateStructure<IFilterSettings>();
                        var formattedValue = GetSearchFormatted(column.Value);

                        filterSettings.Set
                        (
                            item.Value.Data,
                            formattedValue,
                            associates.Count == 1 ? Operand.And : Operand.Or,
                            "contains"
                        );

                        filterSettingsCollection.Add(filterSettings);
                    }
                }
            }
        }

        private void AddOrderSettings(List<ISorterSettings> sorterSettingsCollection)
        {
            foreach (var order in _Orders)
            {
                var column = _Columns.FirstOrDefault(o => o.Key == order.Key);
                var associates = GetAssociates(column);

                foreach (var item in associates)
                {
                    if (item.Value.Orderable == true)
                    {
                        var sortSettings = DatagridSettings.Factory.CreateStructure<ISorterSettings>();

                        sortSettings.Set(item.Value.Data, order.Value);

                        sorterSettingsCollection.Add(sortSettings);
                    }
                }
            }
        }

        private void AddGlobalSearchSettings(List<IFilterSettings> filterSettingsCollection, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue) == false)
            {
                foreach (var column in _Columns)
                {
                    var formattedValue = GetSearchFormatted(column.Value);
                    column.Value.SearchValue = formattedValue;
                }
            }
        }

        private string GetSearchFormatted(Column column)
        {
            var value = column.SearchValue;

            if (column.IsValueFormatted == false && (column.Type == "date" || column.Type == "datetime"))
            {
                value = value.Replace("/", "-")
                    .Replace(".", "-");

                var dateAndTime = value.Split(new char[] { ' ' });

                value = GetDateFormatted(dateAndTime[0]);

                if (dateAndTime.Length > 1)
                {
                    value += " " + dateAndTime[1];
                }
            }

            return value;
        }

        private string GetDateFormatted(string value)
        {
            var date = value.Split(new char[] { '-' });

            return string.Join("-", date.Reverse());
        }

        private List<KeyValuePair<int, Column>> GetAssociates(KeyValuePair<int, Column> column)
        {
            var associates = new List<KeyValuePair<int, Column>>()
            {
                column
            };

            if (column.Value.Associates != null)
            {
                var associatesColumns = column.Value.Associates
                    .Select(o => _Columns.FirstOrDefault(p => p.Value.Data == o))
                    .Where(o => o.Value != null);
                associates.AddRange(associatesColumns);
            }

            return associates;
        }

        private Column GetColumn(IOrderedEnumerable<KeyValuePair<string, string>> parametros, int index)
        {
            var column = new Column();

            foreach (var item in parametros.Where(o => o.Key.StartsWith("columns[" + index + "]")))
            {
                var value = (item.Value ?? "").Trim().ToLower();

                if (item.Key.EndsWith(".data") || item.Key.EndsWith("[data]"))
                {
                    column.Data = value.Replace(".0.", ".");
                }
                else if (item.Key.EndsWith(".name") || item.Key.EndsWith("[name]"))
                {
                    column.Name = value;
                }
                else if (item.Key.EndsWith(".type") || item.Key.EndsWith("[type]"))
                {
                    column.Type = value;
                }
                else if (item.Key.EndsWith(".orderable") || item.Key.EndsWith("[orderable]"))
                {
                    column.Orderable = bool.Parse(value);
                }
                else if (item.Key.EndsWith(".searchable") || item.Key.EndsWith("[searchable]"))
                {
                    column.Searchable = bool.Parse(value);
                }
                else if (item.Key.EndsWith(".search.regex") || item.Key.EndsWith("[search][data]"))
                {
                    column.SearchRegex = value;
                }
                else if (item.Key.EndsWith(".search.value") || item.Key.EndsWith("[search][value]"))
                {
                    column.SearchValue = value;
                }
                else if (item.Key.EndsWith(".associates") || item.Key.EndsWith("[associates]"))
                {
                    column.Associates = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(o => char.ToLowerInvariant(o[0]) + o.Substring(1));
                }
            }

            return column;
        }
    }
}
