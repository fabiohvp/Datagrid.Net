using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Datagrid.Net.Structures
{
    public interface IDatagridResult<TDatagridViewModel>
       where TDatagridViewModel : class
    {
        IDatagridSettings Settings { get; set; }
        bool Cache { get; set; }
        int RecordsTotal { get; set; }
        int RecordsFiltered { get; set; }
        bool Filtered { get; }
        TimeSpan TotalCountTime { get; set; }
        TimeSpan SelectCountTime { get; set; }
        TimeSpan SelectTime { get; set; }
        TimeSpan TotalTime { get; }
        IEnumerable<TDatagridViewModel> Data { get; set; }
        IDatagridResult<TOtherDatagridViewModel> ModifyData<TOtherDatagridViewModel>(Func<IQueryable<TDatagridViewModel>, IEnumerable<TOtherDatagridViewModel>> query)
            where TOtherDatagridViewModel : class;

        IDatagridResult<TDatagridViewModel> ClearCache(string cacheKeyPrefix = default(string), string separator = DatagridSettings.CACHE_SEPARATOR);
        IDatagridResult<TDatagridViewModel> ClearCache(string cacheKeyPrefix);
        IDatagridResult<TDatagridViewModel> ClearCache(Regex regexCacheKeyPrefix);
        IDatagridResult<TDatagridViewModel> SetCache(int timeout);

        object ViewData { get; set; }
        string ToJson();
    }

    public class DatagridResult<TDatagridViewModel> : IDatagridResult<TDatagridViewModel>
        where TDatagridViewModel : class
    {
        [JsonIgnore]
        public virtual IDatagridSettings Settings { get; set; }
        public virtual IEnumerable<TDatagridViewModel> Data { get; set; }
        public virtual bool Cache { get; set; }
        public virtual int RecordsTotal { get; set; }
        public virtual int RecordsFiltered { get; set; }
        public virtual bool Filtered { get { return Settings.ParameterSettings.FilterSettings.Count() > 0; } }

        public virtual TimeSpan TotalCountTime { get; set; }
        public virtual TimeSpan SelectCountTime { get; set; }
        public virtual TimeSpan SelectTime { get; set; }
        public virtual TimeSpan TotalTime
        {
            get
            {
                return TotalCountTime + SelectCountTime + SelectTime;
            }
        }

        public virtual object ViewData { get; set; }

        public virtual IDatagridResult<TOtherDatagridViewModel> ModifyData<TOtherDatagridViewModel>(Func<IQueryable<TDatagridViewModel>, IEnumerable<TOtherDatagridViewModel>> query)
            where TOtherDatagridViewModel : class
        {
            Settings.SetViewModelMapping<TOtherDatagridViewModel>();
            Settings.SetCacheOptions<TOtherDatagridViewModel>();

            var result = Settings.Factory.CreateStructure<IDatagridResult<TOtherDatagridViewModel>>();
            result.Cache = Cache;
            result.Data = query(Data.AsQueryable());
            result.RecordsFiltered = RecordsFiltered;
            result.RecordsTotal = RecordsTotal;
            result.SelectCountTime = SelectCountTime;
            result.SelectTime = SelectTime;
            result.Settings = Settings;
            result.TotalCountTime = TotalCountTime;
            return result;
        }

        public IDatagridResult<TDatagridViewModel> ClearCache(string cacheKeyPrefix = default(string), string separator = DatagridSettings.CACHE_SEPARATOR)
        {
            Settings.ClearCache<TDatagridViewModel>(cacheKeyPrefix, separator);
            return this;
        }

        public IDatagridResult<TDatagridViewModel> ClearCache(string cacheKeyPrefix)
        {
            Settings.ClearCache(cacheKeyPrefix);
            return this;
        }

        public IDatagridResult<TDatagridViewModel> ClearCache(Regex regexCacheKeyPrefix)
        {
            Settings.ClearCache(regexCacheKeyPrefix);
            return this;
        }

        public virtual IDatagridResult<TDatagridViewModel> SetCache(int timeout)
        {
            var cacheKey = Settings.GetCacheKey<TDatagridViewModel>();
            var cache = MemoryCache.Default;

            if (cache.Contains(cacheKey))
            {
                cache.Remove(cacheKey);
            }

            if (timeout == DatagridSettings.CACHE_TIMEOUT_DISABLED)
            {
                return this;
            }

            var policy = new CacheItemPolicy();

            if (timeout == DatagridSettings.CACHE_TIMEOUT_NOT_EXPIRE)
            {
                policy.AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            }
            else
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(timeout);
            }

            cache.Add(cacheKey, this, policy);
            return this;
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Settings.JsonSerializerSettings);
        }
    }
}