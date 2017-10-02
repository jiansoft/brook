using System.Configuration;

//todo need use ConfigurationBuilder
namespace jIAnSoft.Framework.Brook.Configuration
{
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
}
