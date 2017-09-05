using System;
using System.Configuration;
using System.Globalization;

namespace Brook.Configuration
{
    public class Common : ConfigurationElement
    {
        private CultureInfo _culture;

        /// <summary>
        /// 語系設定
        /// </summary>
        [ConfigurationProperty("culture", DefaultValue =  "zh-TW", IsRequired = false)]
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
}
