
namespace jIAnSoft.Framework.Brook.Configuration
{
#if NET451
    using System.Configuration;

    public sealed class DatabaseCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DatabaseSet();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatabaseSet)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.AddRemoveClearMap;
        public static DatabaseCollection Get { get; set; }

        public new DatabaseSet this[string name] => (DatabaseSet)BaseGet(name);

        public bool ContainsKey(string key)
        {
            return BaseGet(key) != null;
        }
    }
#elif NETSTANDARD2_0
    using System.Collections.Generic;

    public class DatabaseCollection
    {
        private static DatabaseCollection _instance;

        public static DatabaseCollection Get => _instance ?? (_instance = new DatabaseCollection());
      

        public DatabaseSet this[string name] => Database[name];

        private Dictionary<string, DatabaseSet> Database { get; set; }

        internal void SetDatabaseCollection(Dictionary<string, DatabaseSet> dic)
        {
            Database = dic;
        }
    }
#endif
}
