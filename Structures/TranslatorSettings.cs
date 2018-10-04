using System.Collections.Generic;

namespace Datagrid.Net.Structures
{
    public interface ITranslatorSettings
    {
        string ColumnName { get; set; }
        string Text { get; set; }
        List<string> Synonyms { get; set; }
    }

    public class TranslatorSettings : ITranslatorSettings
    {
        public string ColumnName { get; set; }

        public string Text { get; set; }

        public List<string> Synonyms { get; set; }

        public TranslatorSettings()
        {
            Synonyms = new List<string>();
        }
    }
}
