using Datagrid.Net.Attributes;
using Datagrid.Net.Factories;
using Datagrid.Net.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Transactions;

namespace Datagrid.Net.Structures
{
    public interface IDatagridSettings
    {
        JsonSerializerSettings JsonSerializerSettings { get; set; }
        Dictionary<string, Func<string, string, bool>> CompareMethods { get; set; }
        Dictionary<Type, Type> Mappings { get; set; }

        IParameterSettings ParameterSettings { get; set; }

        IFactory Factory { get; set; }

        IFilterManager FilterManager { get; set; }
        IParameterManager ParameterManager { get; set; }
        ISorterManager SorterManager { get; set; }
        ITranslatorManager TranslatorManager { get; set; }

        TransactionOptions TransactionOptions { get; set; }
        TransactionScopeOption TransactionScopeOption { get; set; }

        void SetParameters(IDictionary<string, string> parameters, IList<IFilterSettings> filterSettings = default(IList<IFilterSettings>), IList<ISorterSettings> sorterSettings = default(IList<ISorterSettings>));
        void SetParameters(NameValueCollection parameters, IList<IFilterSettings> filterSettings = default(IList<IFilterSettings>), IList<ISorterSettings> sorterSettings = default(IList<ISorterSettings>));

        ObjectCache Cache { get; set; }
        string CacheKeyPrefix { get; set; }
        string CacheSeparator { get; set; }
        int CacheTimeoutInSeconds { get; set; }

        int ClearCache<TDatagridViewModel>(string cacheKeyPrefix = default(string), string separator = DatagridSettings.CACHE_SEPARATOR)
            where TDatagridViewModel : class;
        int ClearCache(string cacheKeyPrefix);
        int ClearCache(Regex regexCacheKeyPrefix);
        string GetCacheKey<TDatagridViewModel>()
            where TDatagridViewModel : class;

        void SetCacheOptions<TDatagridViewModel>()
            where TDatagridViewModel : class;
        void SetViewModelMapping<TDatagridViewModel>()
            where TDatagridViewModel : class;
    }

    public class DatagridSettings : IDatagridSettings
    {
        public const int CACHE_TIMEOUT_NOT_EXPIRE = 0;
        public const int CACHE_TIMEOUT_DISABLED = -1;
        public const string CACHE_SEPARATOR = "#";

        public static string DefaultCacheKeyPrefix = nameof(Datagrid);
        public static int DefaultCacheTimeoutInSeconds = CACHE_TIMEOUT_DISABLED;

        /// <summary>
        /// Methods for comparison of strings, this dictionary include: contains, startswith, endswith
        /// </summary>
        public static Dictionary<string, Func<string, string, bool>> DefaultCompareMethods = new Dictionary<string, Func<string, string, bool>>()
        {
            { "contains", (columnText, filterText) => { return columnText.Contains(filterText); } },
            { "startswith", (columnText, filterText) => { return string.IsNullOrEmpty(filterText) || columnText.StartsWith(filterText); } },
            { "endswith", (columnText, filterText) => { return string.IsNullOrEmpty(filterText) || columnText.EndsWith(filterText); } }
        };

        public JsonSerializerSettings JsonSerializerSettings { get; set; }
        public virtual Dictionary<string, Func<string, string, bool>> CompareMethods { get; set; }
        public virtual Dictionary<Type, Type> Mappings { get; set; }

        public virtual IParameterSettings ParameterSettings { get; set; }

        public virtual IFactory Factory { get; set; }

        public virtual ITranslatorManager TranslatorManager { get; set; }
        public virtual IFilterManager FilterManager { get; set; }
        public virtual ISorterManager SorterManager { get; set; }
        public virtual IParameterManager ParameterManager { get; set; }

        public virtual TransactionOptions TransactionOptions { get; set; }
        public virtual TransactionScopeOption TransactionScopeOption { get; set; }

        public ObjectCache Cache { get; set; }
        public string CacheSeparator { get; set; }
        public virtual string CacheKeyPrefix { get; set; }
        public virtual int CacheTimeoutInSeconds { get; set; }

        public DatagridSettings(
            IDictionary<string, string> parameters = default(IDictionary<string, string>),
            IFactory factory = default(IFactory),
            string cacheKeyPrefix = default(string),
            string cacheSeparator = CACHE_SEPARATOR,
            int? cacheTimeoutInSeconds = default(int?),
            TransactionOptions transactionOptions = default(TransactionOptions),
            TransactionScopeOption transactionScopeOption = default(TransactionScopeOption),
            IDictionary<string, Func<string, string, bool>> compareMethods = default(IDictionary<string, Func<string, string, bool>>),
            IDictionary<Type, Type> mappings = default(IDictionary<Type, Type>),
            IList<IFilterSettings> filterSettings = default(IList<IFilterSettings>),
            IList<ISorterSettings> sorterSettings = default(IList<ISorterSettings>),
            JsonSerializerSettings jsonSerializerSettings = default(JsonSerializerSettings),
            ObjectCache cache = default(ObjectCache)
        )
        {
            if (cache == default(ObjectCache))
            {
                cache = MemoryCache.Default;
            }

            CacheKeyPrefix = cacheKeyPrefix;
            CacheSeparator = cacheSeparator;
            CacheTimeoutInSeconds = cacheTimeoutInSeconds.GetValueOrDefault(DefaultCacheTimeoutInSeconds);

            SetMappings(mappings);
            SetCompareMethods(compareMethods);
            SetFactory(factory);
            SetTransactionOptions(transactionOptions, transactionScopeOption);

            FilterManager = Factory.CreateManager<IFilterManager>();
            SorterManager = Factory.CreateManager<ISorterManager>();
            TranslatorManager = Factory.CreateManager<ITranslatorManager>();
            ParameterManager = Factory.CreateManager<IParameterManager>();

            if (jsonSerializerSettings == default(JsonSerializerSettings))
            {
                jsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Local,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
            }

            JsonSerializerSettings = jsonSerializerSettings;
            SetParameters(parameters, filterSettings, sorterSettings);
        }

        public void SetParameters(IDictionary<string, string> parameters, IList<IFilterSettings> filterSettings = default(IList<IFilterSettings>), IList<ISorterSettings> sorterSettings = default(IList<ISorterSettings>))
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }
            else
            {
                ParameterSettings = ParameterManager.Process(parameters);
            }

            if (filterSettings != null)
            {
                ParameterSettings.FilterSettings.InsertRange(0, filterSettings);
            }

            if (sorterSettings != null)
            {
                ParameterSettings.SorterSettings.InsertRange(0, sorterSettings);
            }
        }

        public void SetParameters(NameValueCollection parameters, IList<IFilterSettings> filterSettings = default(IList<IFilterSettings>), IList<ISorterSettings> sorterSettings = default(IList<ISorterSettings>))
        {
            var parameters2 = new Dictionary<string, string>();

            foreach (var key in parameters.AllKeys)
            {
                parameters2.Add(key, parameters[key]);
            }

            SetParameters(parameters2, filterSettings, sorterSettings);
        }

        private void SetCompareMethods(IDictionary<string, Func<string, string, bool>> compareMethods)
        {
            CompareMethods = DatagridSettings.DefaultCompareMethods;

            if (compareMethods != null)
            {
                foreach (KeyValuePair<string, Func<string, string, bool>> compareMethod in compareMethods)
                {
                    if (CompareMethods.ContainsKey(compareMethod.Key))
                    {
                        CompareMethods.Remove(compareMethod.Key);
                    }

                    CompareMethods.Add(compareMethod.Key, compareMethod.Value);
                }
            }
        }

        private void SetFactory(IFactory factory)
        {
            if (factory == null)
            {
                factory = new Factory(this);
            }

            Factory = factory;
        }

        private void SetMappings(IDictionary<Type, Type> mappings)
        {
            Mappings = new Dictionary<Type, Type>()
            {
                { typeof(IDatagrid), typeof(Datagrid) },
                { typeof(IFilterSettings), typeof(FilterSettings) },
                { typeof(ISorterSettings), typeof(SorterSettings) },
                { typeof(IParameterSettings), typeof(ParameterSettings) },

                { typeof(IFilterManager), typeof(FilterManager) },
                { typeof(ISorterManager), typeof(SorterManager) },
                { typeof(ITranslatorManager), typeof(TranslatorManager) },
                { typeof(IParameterManager), typeof(ParameterManager) },
                
                //{ typeof(IDatagridResult<TDatagridViewModel>), typeof(DatagridResult<TDatagridViewModel>) }
            };

            if (mappings != null)
            {
                foreach (KeyValuePair<Type, Type> mapping in mappings)
                {
                    if (Mappings.ContainsKey(mapping.Key))
                    {
                        Mappings.Remove(mapping.Key);
                    }

                    Mappings.Add(mapping.Key, mapping.Value);
                }
            }
        }

        private void SetTransactionOptions(TransactionOptions transactionOptions, TransactionScopeOption transactionScopeOption)
        {
            if (transactionOptions == default(TransactionOptions))
            {
                transactionOptions = new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadUncommitted
                };
            }

            if (transactionScopeOption == default(TransactionScopeOption))
            {
                transactionScopeOption = TransactionScopeOption.RequiresNew;
            }

            TransactionOptions = transactionOptions;
            TransactionScopeOption = transactionScopeOption;
        }

        public virtual void SetCacheOptions<TDatagridViewModel>()
            where TDatagridViewModel : class
        {
            var cacheAttribute = typeof(TDatagridViewModel).GetFirstAttribute<DatagridCacheAttribute>();

            CacheKeyPrefix = cacheAttribute == default(DatagridCacheAttribute) ? CacheKeyPrefix : cacheAttribute.CacheKeyPrefix;
            CacheTimeoutInSeconds = cacheAttribute == default(DatagridCacheAttribute) ? CacheTimeoutInSeconds : cacheAttribute.CacheTimeoutInSeconds;
        }

        public virtual void SetViewModelMapping<TDatagridViewModel>()
            where TDatagridViewModel : class
        {
            if (Mappings.ContainsKey(typeof(IDatagridResult<TDatagridViewModel>)) == false)
            {
                Mappings.Add(typeof(IDatagridResult<TDatagridViewModel>), typeof(DatagridResult<TDatagridViewModel>));
            }
        }

        public int ClearCache<TDatagridViewModel>(string cacheKeyPrefix = default(string), string separator = CACHE_SEPARATOR)
            where TDatagridViewModel : class
        {
            if (cacheKeyPrefix == default(string))
            {
                cacheKeyPrefix = DefaultCacheKeyPrefix;
            }

            var cacheKey = string.Join(separator, cacheKeyPrefix, typeof(TDatagridViewModel).FullName);
            return ClearCache(cacheKey);
        }

        public int ClearCache(string cacheKeyPrefix)
        {
            var total = 0;
            var cacheKeys = Cache.Select(o => o.Key).Where(o => o.Contains(cacheKeyPrefix));

            foreach (string cacheKey in cacheKeys)
            {
                Cache.Remove(cacheKey);
                total++;
            }

            return total;
        }

        //TODO: Test this method
        /// <summary>
        /// Not tested yet!
        /// </summary>
        /// <param name="regexCacheKeyPrefix"></param>
        public int ClearCache(Regex regexCacheKeyPrefix)
        {
            var total = 0;
            var cacheKeys = Cache.Select(o => o.Key).Where(o => regexCacheKeyPrefix.Match(o).Success);

            foreach (string cacheKey in cacheKeys)
            {
                Cache.Remove(cacheKey);
                total++;
            }

            return total;
        }

        public string GetCacheKey<TDatagridViewModel>()
            where TDatagridViewModel : class
        {
            var cacheKey = new List<string>();

            cacheKey.Add(CacheKeyPrefix);

            if (ParameterSettings.Parameters.ContainsKey("URL"))
            {
                cacheKey.Add(ParameterSettings.Parameters["URL"]);
            }

            if (ParameterSettings.Parameters.ContainsKey("QUERY_STRING"))
            {
                cacheKey.Add(ParameterSettings.Parameters["QUERY_STRING"]);
            }

            cacheKey.Add(typeof(TDatagridViewModel).FullName);

            foreach (var filterSettings in ParameterSettings.FilterSettings)
            {
                cacheKey.Add(filterSettings.ColumnName + "@" + filterSettings.CompareMethod + "@" + filterSettings.Operand + "@" + filterSettings.Text);
            }

            foreach (var sorterSettings in ParameterSettings.SorterSettings)
            {
                cacheKey.Add(sorterSettings.ColumnName + "@" + sorterSettings.SortDirection.GetDescription());
            }

            cacheKey.Add(ParameterSettings.PageNumber + "@" + ParameterSettings.PageSize);

            return string.Join(CACHE_SEPARATOR, cacheKey);
        }
    }
}
