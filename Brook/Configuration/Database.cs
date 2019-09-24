namespace jIAnSoft.Brook.Configuration
{
#if NET461
    using System.Configuration;

    public sealed class Database : ConfigurationElement
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DatabaseCollection Which => base[""] as DatabaseCollection;
    }

#elif NETSTANDARD2_0 || NETSTANDARD2_1

    public class DatabaseWrap
    {
        public DatabaseCollection Which => DatabaseCollection.Get;
    }

#endif
}