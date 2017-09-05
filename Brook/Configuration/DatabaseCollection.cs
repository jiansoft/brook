using System.Configuration;

namespace Brook.Configuration
{
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

        public new DatabaseSet this[string name] => (DatabaseSet)BaseGet(name);

        public bool ContainsKey(string key)
        {
            return BaseGet(key) != null;
        }
    }
}
