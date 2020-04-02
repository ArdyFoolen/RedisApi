using System.Configuration;

namespace RedisApi.Configuration
{
    public class RedisConfigurationManager
    {
        #region Constants

        private const string SectionName = "RedisConfiguration";

        public static RedisConfigurationSection Config
        {
            get
            {
                return (RedisConfigurationSection)ConfigurationManager.GetSection(SectionName);
            }
        }

        #endregion
    }
}
