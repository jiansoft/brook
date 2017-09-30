using jIAnSoft.Framework.Brook.Configuration;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;

namespace jIAnSoft.Framework.Brook
{
    public sealed class MsSql : DbProvider
    {
        public MsSql(string argStrDbProviderName)
            : base(argStrDbProviderName)
        {
        }

        public MsSql(string argStrHost, string argStrDbName, string argStrUser, string argStrPassword)
            : this(argStrHost, 1433, argStrDbName, argStrUser, argStrPassword)
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
        public MsSql(string argStrHost, int argIntPort, string argStrDbName, string argStrUser, string argStrPassword)
            : base(new ConnectionStringSettings
            {
                ConnectionString =
                    $"server={argStrHost},{argIntPort};database={argStrDbName};uid={argStrUser};pwd={argStrPassword};",
                ProviderName = "System.Data.SqlClient",
                Name = argStrDbName
            })
        {
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        public void ChangeConnection(string argStrDbProviderName)
        {
            InitDbConfig(argStrDbProviderName);
        }

        public void Dispose(bool disposing)
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

       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="batchSize"></param>
        public void Bulk(DataTable table, int batchSize = 2500)
        {
            using (var bulk = new SqlBulkCopy(ConnectionSource))
            {
                bulk.BatchSize = batchSize;
                bulk.BulkCopyTimeout = 0;
                bulk.DestinationTableName = $"[dbo].[{table.TableName}]";
                foreach (DataColumn column in table.Columns)
                {
                    bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                try
                {
                    // Write from the source to the destination.
                    bulk.WriteToServer(table);
                }
                catch (Exception sqlEx)
                {
                    throw new SqlException(
                        string.Format(
                            new CultureInfo(Section.Get.Common.Culture.Name),
                            "{0} Source = {1}\n Table = {2}\n",
                            sqlEx.Message,
                            DbConfig.Name,
                            table.TableName
                        ),
                        sqlEx);
                }
            }
        }
    }
}
