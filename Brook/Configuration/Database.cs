namespace jIAnSoft.Brook.Configuration
{
#if NET461
    using System.Configuration;

    public sealed class Database : ConfigurationElement
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DatabaseCollection Which => base[""] as DatabaseCollection;
    }

#else

    public class DatabaseWrap
    {
        public DatabaseCollection Which => DatabaseCollection.Get;
    }

#endif
}