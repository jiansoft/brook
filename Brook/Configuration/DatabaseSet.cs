﻿using System;
using System.Configuration;

namespace jIAnSoft.Framework.Brook.Configuration
{
    public sealed class DatabaseSet : ConfigurationElement
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        [ConfigurationProperty("connection")]
        public string Connection => (string)base["connection"];

        /// <summary>
        /// 識別名稱
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => (string)base["name"];

        /// <summary>
        /// 取得 .NET Framework 資料提供者的名稱，關聯的 SqlDataSource 控制項會用來連接到基礎資料來源。
        /// </summary>
        [ConfigurationProperty("providerName", IsRequired = true)]
        public string ProviderName => (string)base["providerName"];

        /// <summary>
        /// 資料庫連線逾時設定
        /// </summary>
        [ConfigurationProperty("commandTimeOut", DefaultValue = "30")]
        public int CommandTimeOut => Convert.ToInt32(base["commandTimeOut"]);
    }
}
