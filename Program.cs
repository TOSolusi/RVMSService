
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using RVMSService.Data;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace RVMSService
{
    public class Program
    {

        public static void Main(string[] args)
        {
            //Setting up to read json settings file and ensure that it will read on the application directory
            
            //string sharedSettingsPath = Path.Combine(exeDirectory, @"Settings\Settings.json");
            //sharedSettingsPath = Path.GetFullPath(sharedSettingsPath);

            //Configure Serilog to log to a file
           
            
          

                var builder = WebApplication.CreateBuilder(args);

                //string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                //string logPath = Path.Combine(exeDirectory, "Logs", "log-.txt");
                //Log.Logger = new LoggerConfiguration()
                //    .WriteTo.Console()
                //    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                //    .CreateLogger();


                builder.Host.UseWindowsService();
            // Use Serilog as the logging provider
            builder.Host.UseSerilog((context, services, configuration) =>
                {
                    var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var logPath = Path.Combine(exeDirectory, "Logs", "log-.txt");
                    configuration
                        .WriteTo.Console()
                        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
                });

                builder.Services.AddDbContext<AppDBContext>(option =>
                {
                    option.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"));
                    //option.UseInMemoryDatabase("AuthDb");
                });

                // Add services to the container.
                builder.Services.AddAuthorization();
                builder.Services.AddIdentityApiEndpoints<IdentityUser>()
                    .AddEntityFrameworkStores<AppDBContext>();


                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Auth Demo",
                        Version = "v1"

                    });
                    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                    {
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Please Enter the Token",
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "bearer"
                    });

                    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        []
                    }
                });
                }
                );


                var app = builder.Build();

                app.MapIdentityApi<IdentityUser>();
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

        //    public static JObject SettingsConfig { get; set; }
        //    public static string FileSettings { get; set; }
        //    public string ConnString { get; set; }
        //    public string serverAddress { get; set; }

        //    public static void Main(string[] args)
        //    {
        //        

        //        // Configure Serilog to log to a file
        //        string logPath = Path.Combine(exeDirectory, "Logs", "log-.txt");
        //        Log.Logger = new LoggerConfiguration()
        //            .WriteTo.Console()
        //            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
        //            .CreateLogger();

        //        try
        //        {


        //            SettingsConfig = JObject.Parse(System.IO.File.ReadAllText(sharedSettingsPath));
        //            //string httpUrl = $"http://{SettingsConfig["ServerAddresshttp"]?.ToString() ?? "localhost"}";
        //            //string httpsUrl = $"https://{SettingsConfig["ServerAddresshttps"]?.ToString() ?? "localhost"}";

        //            //$"Server={StringServer};Database={StringDatabase}; Integrated Security={IntegratedSecurity}; Encrypt=false;";
        //            // string connectionString = $"Server={SettingsConfig["Server"].ToString()};Database={SettingsConfig["Database"].ToString()}; Integrated Security={SettingsConfig["IntegratedSecurity"].ToString()}; Encrypt=false;";    //SettingsConfig["ConnectionString"]?.ToString() ?? "";
        //            Log.Information($"Starting web host");

        //            //string connectionString = SettingsConfig["ConnectionString"].ToString();




        //            var builder = WebApplication.CreateBuilder(args);


        //            builder.Host.UseWindowsService();
        //            builder.Host.UseSerilog();

        //            //builder.WebHost.UseUrls(httpUrl, httpsUrl);
        //            //builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        //            //builder.Configuration.AddJsonFile(sharedSettingsPath, optional: false, reloadOnChange: true);

        //            //var connectionString = builder.Configuration.GetConnectionString($"Server={SettingsConfig["Server"].ToString()};Database={SettingsConfig["Database"].ToString()}; Integrated Security={SettingsConfig["IntegratedSecurity"].ToString()}; Encrypt=false;") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        //            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        //                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


        //            builder.Services.AddDbContext<AppDBContext>(options =>
        //              options.UseSqlServer(connectionString));



        //            builder.Services.AddAuthorization();

        //            builder.Services.AddIdentityApiEndpoints<IdentityUser>()
        //                .AddEntityFrameworkStores<AppDBContext>();



        //            // Add services to the container.
        //            builder.Services.AddControllers();
        //            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        //            builder.Services.AddEndpointsApiExplorer();

        //            //builder.Services.AddSwaggerGen();


        //            builder.Services.AddSwaggerGen(options =>
        //            {
        //                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
        //                {
        //                    Title = "RVMS",
        //                    Version = "v1"

        //                });
        //                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
        //                {
        //                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        //                    Description = "Please Enter the Token",
        //                    Name = "Authorization",
        //                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        //                    BearerFormat = "JWT",
        //                    Scheme = "bearer"
        //                });

        //                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        //            {
        //                {
        //                    new OpenApiSecurityScheme
        //                    {
        //                        Reference = new OpenApiReference
        //                        {
        //                            Type = ReferenceType.SecurityScheme,
        //                            Id = "Bearer"
        //                        }
        //                    },
        //                    []
        //                }
        //            });
        //            }
        //        );


        //            var app = builder.Build();

        //            app.MapIdentityApi<IdentityUser>();

        //            // Configure the HTTP request pipeline.
        //            if (app.Environment.IsDevelopment())
        //            {
        //                app.UseSwagger();
        //                app.UseSwaggerUI();
        //            }

        //            app.UseHttpsRedirection();


        //            app.UseAuthentication();
        //            app.UseAuthorization();

        //            app.MapControllers();

        //            app.Run();
        //        }
        //        catch (Exception ex)
        //        {
        //            
        //        }
        //        finally
        //        {
        //           
        //        }
        //    }
    }
}
