

using System;
using System.Globalization;

namespace jIAnSoft.Framework.Brook.Configuration
{
#if NET451
    using System.Configuration;

    public class Common : ConfigurationElement
    {
        private CultureInfo _culture;

        /// <summary>
        /// 語系設定
        /// </summary>
        [ConfigurationProperty("culture", DefaultValue = "zh-TW", IsRequired = false)]
        public CultureInfo Culture
        {
            get
            {
                try
                {
                    _culture = new CultureInfo(Convert.ToString(base["culture"]));
                }
                catch (Exception)
                {
                    _culture = new CultureInfo("zh-TW");
                }
                return _culture;
            }
        }


        [ConfigurationProperty("timezone", DefaultValue = "Taipei Standard Time", IsRequired = false)]
        private string Timezone => Convert.ToString(base["timezone"]);

        private static TimeZoneInfo _timeZoneInfo;


        /// <summary>
        /// 時區設定
        /// </summary>
        /* [ConfigurationProperty("zone", IsRequired = false)]*/
        public TimeZoneInfo TimeZone
        {
            get
            {
                try
                {
                    return _timeZoneInfo ?? (_timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Timezone));
                }
                catch (Exception)
                {
                    return _timeZoneInfo ??
                           (_timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"));

                }
            }
        }

        /// <summary>
        /// 應用程式名稱
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => Convert.ToString(base["name"]);

    }
#elif NETSTANDARD2_0
    public class Common 
    {
        public Common(string culture, string timezone)
        {
            try
            { 
                 TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            catch (Exception)
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
            }
            try
            {
                Culture = new CultureInfo(culture);
            }
            catch (Exception)
            {
                Culture = new CultureInfo("zh-TW");
            }
        }

        /// <summary>
        /// 語系設定
        /// </summary>
        public CultureInfo Culture { get; set; }
        /// <summary>
        /// 時區設定
        /// </summary>
        /* [ConfigurationProperty("zone", IsRequired = false)]*/
        public TimeZoneInfo TimeZone{ get; set; }

        public string Name { get; set; }
    }
#endif
}
