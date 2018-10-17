using System.ComponentModel;

namespace jIAnSoft.Brook.Configuration
{
#if NET451
    using System;
    using System.Configuration;
    public sealed class DatabaseConfiguration : ConfigurationElement
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        [ConfigurationProperty("connection")]
        public string Connection
        {
            get { return (string)base["connection"]; }
            set { base["connection"] = value; }
        }

        /// <summary>
        /// 識別名稱
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string) base["name"]; }
            set { base["name"] = value; }
        }

        /// <summary>
        /// 取得 .NET Framework 資料提供者的名稱，關聯的 SqlDataSource 控制項會用來連接到基礎資料來源。
        /// </summary>
        [ConfigurationProperty("providerName", IsRequired = true)]
        public string ProviderName
        {
            get { return (string)base["providerName"]; }
            set { base["providerName"] = value; }
        }

        /// <summary>
        /// 資料庫連線逾時設定
        /// </summary>
        [ConfigurationProperty("commandTimeout", DefaultValue = "5")]
        public int CommandTimeout
        {
            get { return Convert.ToInt32(base["commandTimeout"]); }
            set { base["commandTimeout"] = value.ToString(); }
        }
    }

#elif NETSTANDARD2_0
    public sealed class DatabaseConfiguration 
    {
        public string Connection { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }
        [DefaultValue(5)]
        public int CommandTimeout { get; set; }
    }
#endif
}
