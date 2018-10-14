namespace jIAnSoft.Brook.Configuration
{
#if NET451
    using System.Configuration;

    public sealed class Database : ConfigurationElement
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DatabaseCollection Which => base[""] as DatabaseCollection;
    }

#elif NETSTANDARD2_0

    public class DatabaseWrap
    {
        public DatabaseCollection Which => DatabaseCollection.Get;
    }

#endif
}