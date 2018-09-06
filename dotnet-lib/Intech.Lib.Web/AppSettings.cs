#region Usings
using Intech.Lib.Util.Email;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO; 
#endregion

namespace Intech.Lib.Web
{
    public class AppSettings
    {
        public static string NomeArquivo = "appsettings.json";

        public string ConnectionProvider { get; set; }

        public string ConnectionString { get; set; }

        public ConfigEmail Email { get; set; }

        public string PublicacaoPortal { get; set; }

        public string PublicacaoAPI { get; set; }

        public static AppSettings Get()
        {
            var file = File.ReadAllText(NomeArquivo);
            return JsonConvert.DeserializeObject<AppSettings>(file);
        }

        public static bool IS_SQL_SERVER_PROVIDER => Get().ConnectionProvider == "sqlserver";
        public static bool IS_ORACLE_PROVIDER => Get().ConnectionProvider == "oracle";
    }
}
