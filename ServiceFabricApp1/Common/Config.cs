using System.Configuration;

namespace Common
{
    public class Config
    {
        public static string TestConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["TestConnectionString"].ConnectionString;
            }
        }
    }
}