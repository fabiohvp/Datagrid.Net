using Datagrid.Net.Structures;
using System.Text.RegularExpressions;

namespace Datagrid.Net.Managers
{
    public class DatagridManager
    {
        private static IDatagrid _Instance = default(IDatagrid);

        private static object ObjectLock = new object();

        private DatagridManager()
        { }

        public static IDatagrid Instance
        {
            get
            {
                Create();

                return _Instance;
            }
        }

        public static void Create(IDatagridSettings settings = default(IDatagridSettings))
        {
            lock (ObjectLock) //single - check lock
            {
                if (_Instance == default(IDatagrid))
                {
                    if (settings == default(IDatagridSettings))
                    {
                        settings = new DatagridSettings();
                    }
                    _Instance = new Datagrid(settings);
                }
            }
        }

        public static int ClearCache<TDatagridViewModel>(string cacheKeyPrefix = default(string), string separator = DatagridSettings.CACHE_SEPARATOR)
            where TDatagridViewModel : class
        {
            return _Instance.Settings.ClearCache<TDatagridViewModel>(cacheKeyPrefix, separator);
        }

        public static int ClearCache(string cacheKeyPrefix)
        {
            return _Instance.Settings.ClearCache(cacheKeyPrefix);
        }

        public static int ClearCache(Regex regexCacheKeyPrefix)
        {
            return _Instance.Settings.ClearCache(regexCacheKeyPrefix);
        }

        public static string GetCacheKey<TDatagridViewModel>()
            where TDatagridViewModel : class
        {
            return _Instance.Settings.GetCacheKey<TDatagridViewModel>();
        }
    }
}