
using Newtonsoft.Json.Linq;
using Serilog;

namespace RVMSService
{
    public class Program
    {

        public static JObject SettingsConfig { get; set; }
        public static string FileSettings { get; set; }
        public string ConnString { get; set; }
        public string serverAddress { get; set; }

        public static void Main(string[] args)
        {
            //Setting up to read json settings file and ensure that it will read on the application directory
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sharedSettingsPath = Path.Combine(exeDirectory, @"Settings\Settings.json");
            sharedSettingsPath = Path.GetFullPath(sharedSettingsPath);

            // Configure Serilog to log to a file
            string logPath = Path.Combine(exeDirectory, "Logs", "log-.txt");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                SettingsConfig = JObject.Parse(System.IO.File.ReadAllText(sharedSettingsPath));
                string url = $"http://{SettingsConfig["ServerAddress"]?.ToString() ?? "localhost"}";

                Log.Information("Starting web host");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseWindowsService();
                builder.Host.UseSerilog();
                builder.WebHost.UseUrls(url);


                // Add services to the container.
                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
