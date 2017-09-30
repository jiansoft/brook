using System.Configuration;

namespace jIAnSoft.Framework.Brook
{
    public class MySql : DbProvider
    {
        public MySql(string argStrDbProviderName) : base(argStrDbProviderName)
        {
        }

        public MySql(string argStrHost, string argStrDbName, string argStrUser, string argStrPassword)
            : this(argStrHost, 3306, argStrDbName, argStrUser, argStrPassword)
        {
        }
        
        /// <inheritdoc />
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argStrHost"></param>
        /// <param name="argIntPort"></param>
        /// <param name="argStrUser"></param>
        /// <param name="argStrPassword"></param>
        /// <param name="argStrDbName"></param>
        public MySql(string argStrHost, int argIntPort, string argStrDbName, string argStrUser, string argStrPassword)
            : base(new ConnectionStringSettings
            {
                ConnectionString =
                    $"server={argStrHost},{argIntPort};database={argStrDbName};uid={argStrUser};pwd={argStrPassword};",
                ProviderName = "MySql.Data.MySqlClient",
                Name = argStrDbName
            })
        {
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || Disposed) return;

            if (null != Conn)
            {
                Conn.Close();
                Conn.Dispose();
                Conn = null;
            }
            Disposed = true;
        }

        public override void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        //Brook.Fetch("dbname").Table(");
    }

    /*public class Brook
    {
        private static Brook _instance;

        private static Brook Instance => _instance ?? (_instance = new Brook());

        private Brook()
        {
        }

        public static DataTable Table(int interval)
        {
            return new DataTable();
        }

    }*/
}
