using Datagrid.Net.Structures;
using System;

namespace Datagrid.Net.Attributes
{
    public class DatagridCacheAttribute : Attribute
    {
        public readonly string CacheKeyPrefix;
        public readonly int CacheTimeoutInSeconds;

        public DatagridCacheAttribute(string cacheKeyPrefix = default(string))
        {
            CacheKeyPrefix = cacheKeyPrefix;
            CacheTimeoutInSeconds = DatagridSettings.DefaultCacheTimeoutInSeconds;
        }

        public DatagridCacheAttribute(int cacheTimeoutInSeconds, string cacheKeyPrefix = default(string))
        {
            CacheKeyPrefix = cacheKeyPrefix;
            CacheTimeoutInSeconds = cacheTimeoutInSeconds;
        }
    }
}
