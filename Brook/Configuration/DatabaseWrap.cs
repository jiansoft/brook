namespace jIAnSoft.Framework.Brook.Configuration
{
#if NETSTANDARD2_0
    public class DatabaseWrap
    {
        public DatabaseCollection Which => DatabaseCollection.Get;
    }
#endif
}