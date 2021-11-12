using System;

namespace jIAnSoft.Brook.Configuration
{
#if NET461
    using System.Configuration;

    public class Section : ConfigurationSection
    {
        private static Section _instance;

        public static Section Get =>
            _instance ?? (_instance = (Section) ConfigurationManager.GetSection("jIAnSoft/framework"));
        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("database")]
        public Database Database => (Database) base["database"];
        
        /// <summary>
        /// 通用設定
        /// </summary>
        [ConfigurationProperty("common", IsRequired = true)]
        public Common Common => (Common) base["common"];
    }
#elif NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0 || NET6_0
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Section
    {
        private IConfigurationRoot Figuration { get; set; }
        private static Section _instance;

        public static Section Get => _instance ?? (_instance = new Section());

        private Section()
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);
            if (File.Exists("app.json"))
            {
                builder.AddJsonFile("app.json", true, true);
            }
            else if (File.Exists("appsettings.json"))
            {
                builder.AddJsonFile("appsettings.json", true, true);
            }

            Figuration = builder.Build();

            var co = new List<DatabaseConfiguration>();
            Figuration.GetSection("Database").Bind(co);
            var c = new Common(Figuration["Common:Culture"], Figuration["Common:Timezone"], Figuration["Common:Name"]);
            Figuration.GetSection("Common").Bind(c);
            Common = c;
            Database = new DatabaseWrap();
            Database.Which.SetDatabaseCollection(co.ToDictionary(m => m.Name, m => m));
        }

        public Common Common { get; }
        public DatabaseWrap Database { get; }
    }
#endif
}
