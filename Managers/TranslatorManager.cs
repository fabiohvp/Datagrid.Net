using Datagrid.Net.Structures;
using System.Collections.Generic;
using System.Linq;

namespace Datagrid.Net.Managers
{
    public interface ITranslatorManager : IManager
    {
        List<ITranslatorSettings> Dictionary { get; set; }

        string GetTranslation(string columnName, string text);
    }

    public class TranslatorManager : ITranslatorManager
    {
        public virtual List<ITranslatorSettings> Dictionary { get; set; }

        public virtual IDatagridSettings DatagridSettings { get; protected set; }

        public TranslatorManager(IDatagridSettings datagridSettings)
        {
            Dictionary = new List<ITranslatorSettings>();
            DatagridSettings = datagridSettings;
        }

        public virtual string GetTranslation(string columnName, string text)
        {
            foreach (var translatorSettings in Dictionary.Where(o => o.ColumnName.ToLower() == columnName))
            {
                foreach (var synonym in translatorSettings.Synonyms)
                {
                    text = text.Replace(synonym, translatorSettings.Text);
                }
            }

            return text;
        }
    }
}
