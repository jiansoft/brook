using System.Configuration;

namespace jIAnSoft.Brook.Configuration
{
    public sealed class Database : ConfigurationElement
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DatabaseCollection Which => base[""] as DatabaseCollection;
    }
}
