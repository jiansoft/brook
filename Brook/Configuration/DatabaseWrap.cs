#if NETSTANDARD2_0
namespace jIAnSoft.Brook.Configuration
{

    public class DatabaseWrap
    {
        public DatabaseCollection Which => DatabaseCollection.Get;
    }

}
#endif