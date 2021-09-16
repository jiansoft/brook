
namespace jIAnSoft.Brook.Configuration
{
#if NET461
    using System.Configuration;

    public sealed class DatabaseCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DatabaseConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatabaseConfiguration)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.AddRemoveClearMap;
        public static DatabaseCollection Get { get; set; }

        public new DatabaseConfiguration this[string name] => (DatabaseConfiguration)BaseGet(name);

        public bool ContainsKey(string key)
        {
            return BaseGet(key) != null;
        }
    }
#elif NETSTANDARD2_0 || NETSTANDARD2_1 || NET5_0 || NET6_0
    using System.Collections.Generic;

    public class DatabaseCollection
    {
        private static DatabaseCollection _instance;

        public static DatabaseCollection Get => _instance ?? (_instance = new DatabaseCollection());

        public DatabaseConfiguration this[string name] => Database[name];

        private Dictionary<string, DatabaseConfiguration> Database { get; set; }

        internal void SetDatabaseCollection(Dictionary<string, DatabaseConfiguration> dic)
        {
            Database = dic;
        }
        public bool ContainsKey(string key)
        {
            return Database.ContainsKey(key);
        }
    }
#endif
}
