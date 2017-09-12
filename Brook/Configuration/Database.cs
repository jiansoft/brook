using System;
using System.Configuration;

namespace jIAnSoft.Framework.Brook.Configuration
{
    public sealed class Database : ConfigurationElement
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DatabaseCollection Which => base[""] as DatabaseCollection;

        /// <summary>
        /// 資料庫連線逾時設定
        /// </summary>
        [ConfigurationProperty("commandTimeOut", DefaultValue = "5")]
        public int CommandTimeOut => Convert.ToInt32(base["commandTimeOut"]);
    }
}
