using System.Configuration;

namespace jIAnSoft.Framework.Brook.Configuration
{
    public sealed class Database : ConfigurationElement
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DatabaseCollection Which => base[""] as DatabaseCollection;
    }
}
