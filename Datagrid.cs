using Datagrid.Net.Structures;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Transactions;

namespace Datagrid.Net
{
    public interface IDatagrid
    {
        IDatagridSettings Settings { get; set; }

        IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IEnumerable<TDatagridViewModel> collection, NameValueCollection parameters)
            where TDatagridViewModel : class;
        IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IEnumerable<TDatagridViewModel> collection, IDictionary<string, string> parameters)
            where TDatagridViewModel : class;
        IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, NameValueCollection parameters)
            where TDatagridViewModel : class;
        IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, IDictionary<string, string> parameters)
            where TDatagridViewModel : class;
    }

    public class Datagrid : IDatagrid
    {
        public virtual IDatagridSettings Settings { get; set; }

        public Datagrid(IDatagridSettings settings)
        {
            Settings = settings;
        }

        public virtual IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IEnumerable<TDatagridViewModel> collection, NameValueCollection parameters)
            where TDatagridViewModel : class
        {
            return Process(collection, parameters.AllKeys.ToDictionary(k => k, v => parameters[v]));
        }

        public virtual IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IEnumerable<TDatagridViewModel> collection, IDictionary<string, string> parameters)
            where TDatagridViewModel : class
        {
            return Process(collection.AsQueryable(), parameters);
        }

        public virtual IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, NameValueCollection parameters)
            where TDatagridViewModel : class
        {
            return Process(query, parameters.AllKeys.ToDictionary(k => k, v => parameters[v]));
        }

        public virtual IDatagridResult<TDatagridViewModel> Process<TDatagridViewModel>(IQueryable<TDatagridViewModel> query, IDictionary<string, string> parameters)
            where TDatagridViewModel : class
        {
            Settings.SetCacheOptions<TDatagridViewModel>();
            Settings.SetViewModelMapping<TDatagridViewModel>();
            Settings.SetParameters(parameters);

            var cacheKey = Settings.GetCacheKey<TDatagridViewModel>();
            var cacheDatagridResult = (IDatagridResult<TDatagridViewModel>)Settings.Cache.Get(cacheKey);

            if (cacheDatagridResult != default(IDatagridResult<TDatagridViewModel>))
            {
                cacheDatagridResult.Cache = true;
                return cacheDatagridResult;
            }

            using (var transactionScope = new TransactionScope(Settings.TransactionScopeOption, Settings.TransactionOptions))
            {
                query = query.AsNoTracking();
                var datagridResult = Settings.Factory.CreateStructure<IDatagridResult<TDatagridViewModel>>();
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                datagridResult.RecordsTotal = query.Count();

                stopwatch.Stop();
                datagridResult.TotalCountTime = stopwatch.Elapsed;

                stopwatch.Restart();

                if (Settings.ParameterSettings.FilterSettings.Any())
                {
                    query = Settings
                        .FilterManager
                        .Process(query, Settings.ParameterSettings.FilterSettings);

                    datagridResult.RecordsFiltered = query.Count();
                }
                else
                {
                    datagridResult.RecordsFiltered = datagridResult.RecordsTotal;
                }

                stopwatch.Stop();
                datagridResult.SelectCountTime = stopwatch.Elapsed;

                stopwatch.Restart();

                query = Settings
                   .SorterManager
                   .Process(query, Settings.ParameterSettings.SorterSettings);

                if (Settings.ParameterSettings.PageSize >= 0)
                {
                    datagridResult.Data = query
                        .Skip((Settings.ParameterSettings.PageNumber - 1) * Settings.ParameterSettings.PageSize)
                        .Take(Settings.ParameterSettings.PageSize)
                        .ToArray();
                }
                else
                {
                    datagridResult.Data = query
                        .ToArray();
                }

                stopwatch.Stop();
                datagridResult.SelectTime = stopwatch.Elapsed;

                datagridResult.Settings = Settings;
                datagridResult.SetCache(Settings.CacheTimeoutInSeconds);

                return datagridResult;
            }
        }
    }
}