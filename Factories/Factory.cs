using Datagrid.Net.Structures;
using System;

namespace Datagrid.Net.Factories
{
    public interface IFactory
    {
        IDatagridSettings DatagridSettings { get; }

        object CreateStructure(Type instanceType);
        TInterface CreateStructure<TInterface>();

        object CreateManager(Type instanceType);
        TInterface CreateManager<TInterface>();
    }

    public class Factory : IFactory
    {
        public IDatagridSettings DatagridSettings { get; protected set; }

        public Factory(IDatagridSettings datagridSettings)
        {
            DatagridSettings = datagridSettings;
        }

        public virtual object CreateStructure(Type instanceType)
        {
            return Activator.CreateInstance(DatagridSettings.Mappings[instanceType]);
        }

        public virtual TInterface CreateStructure<TInterface>()
        {
            return (TInterface)Activator.CreateInstance(DatagridSettings.Mappings[typeof(TInterface)]);
        }


        public virtual object CreateManager(Type instanceType)
        {
            return Activator.CreateInstance(DatagridSettings.Mappings[instanceType], DatagridSettings);
        }

        public virtual TInterface CreateManager<TInterface>()
        {
            return (TInterface)Activator.CreateInstance(DatagridSettings.Mappings[typeof(TInterface)], DatagridSettings);
        }
    }
}
