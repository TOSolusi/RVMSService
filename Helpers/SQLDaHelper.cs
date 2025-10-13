using Newtonsoft.Json.Linq;

namespace RVMSService.Helpers
{
    public class SQLDaHelper
    {
        private readonly IConfiguration _configuration;
        public string StringServer { get; set; }
       // public string StringTargetServer { get; set; }
        public string StringDatabase { get; set; }
        //public string StringTargetDatabase { get; set; }
        public string connString { get; set; }
        //public string targetConnString { get; set; }
        //public JObject SettingsConfig { get; set; }
        //public string FileSettings { get; set; }
        public string sqlCommand { get; set; }
        //public List<int> DoorIn { get; set; }
        //public List<int>  DoorOut { get; set; }
        //public string doorInList { get; set; }
        //public string doorOutList { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string? stringUser { get; set; }
        public string? stringPassword { get; set; }

        public SQLDaHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            StringServer = _configuration.GetValue<string>("Server") ?? "localhost";
            StringDatabase = _configuration.GetValue<string>("Database") ?? "RVMSDB";
            IntegratedSecurity = _configuration.GetValue<bool>("IntegratedSecurity");
            if (IntegratedSecurity)
            {
                connString = $"Server={StringServer};Database={StringDatabase};Integrated Security=True; Encrypt=False;";
            }
            else
            {
                stringUser = _configuration.GetValue<string>("User") ?? "sa";
                stringPassword = _configuration.GetValue<string>("Password") ?? "your_password";
                connString = $"Server={StringServer};Database={StringDatabase};User Id={stringUser};Password={stringPassword};  Encrypt=False;";
            }


        }
        // Add methods to interact with the database using ADO.NET or any ORM of your choice.

    }
}
