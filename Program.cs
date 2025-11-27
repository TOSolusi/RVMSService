
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using RVMSService.Data;
using RVMSService.Models;
using RVMSService.Services;
using Serilog;
using System.Text;

namespace RVMSService
{
    public class Program
    {
        public static JObject SettingsConfig { get; set; }
        public static string FileSettings { get; set; }
        public string ConnString { get; set; }
        public string serverAddress { get; set; }
        public static async Task Main(string[] args)


        {

            //Setting up to read json settings file and ensure that it will read on the application directory
            //Settings file in Settings folder


            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sharedSettingsPath = Path.Combine(exeDirectory, @"Settings\Settings.json");
            sharedSettingsPath = Path.GetFullPath(sharedSettingsPath);


            SettingsConfig = JObject.Parse(System.IO.File.ReadAllText(sharedSettingsPath));
            string httpUrl = $"http://{SettingsConfig["ServerAddresshttp"]?.ToString() ?? "localhost"}";
            //string httpsUrl = $"https://{SettingsConfig["ServerAddresshttps"]?.ToString() ?? "localhost"}";

            //            //$"Server={StringServer};Database={StringDatabase}; Integrated Security={IntegratedSecurity}; Encrypt=false;";
            string connectionString = $"Server={SettingsConfig["Server"].ToString()};Database={SettingsConfig["Database"].ToString()}; Integrated Security={SettingsConfig["IntegratedSecurity"].ToString()}; Encrypt=false;";    //SettingsConfig["ConnectionString"]?.ToString() ?? "";
            string secretKey = SettingsConfig["JwtKey"]?.ToString();
            string issuer = SettingsConfig["JwtIssuer"]?.ToString();

           
                var builder = WebApplication.CreateBuilder(args);

                builder.Configuration.AddJsonFile(sharedSettingsPath, optional: false, reloadOnChange: true);

                //Create as a windows service
                builder.Host.UseWindowsService();
                builder.WebHost.UseUrls(httpUrl);
                // Use Serilog as the logging provider
                builder.Host.UseSerilog((context, services, configuration) =>
                    {
                        var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        var logPath = Path.Combine(exeDirectory, "Logs", "log-.txt");
                        configuration
                            .WriteTo.Console()
                            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
                    });

                //Console.WriteLine($"JWT key length: {Encoding.UTF8.GetBytes(secretKey).Length} bytes");


                builder.Services.AddDbContext<AppDBContext>(option =>
                {
                    option.UseSqlServer(connectionString);
                    //option.UseInMemoryDatabase("AuthDb");
                });

            // Add services to the container.
            builder.Services.AddScoped<IAuditTrailService , AuditTrailService>();
            builder.Services.AddScoped<IGateService, GateService>();
            builder.Services.AddScoped<IDestinationService, DestinationService>();
            builder.Services.AddScoped<IVisitTypeService, VisitTypeService>();
            builder.Services.AddScoped<IQRCodeService, QRCodeService>();
            builder.Services.AddScoped<IVisitorService, VisitorService>();
            builder.Services.AddScoped<IVisitService, VisitService>();
            builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

            builder.Services.AddAuthorization();
                builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = false;  // Disable email confirmation requirement
                                                                   //options.User.RequireUniqueEmail = false;  
                                                                   // Email doesn't need to be unique
                                                                   // Configure password requirements (less restrictive but still somewhat secure)
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 4;              // At least 4 characters
                    options.Password.RequiredUniqueChars = 1;         // At least 1 unique character

                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Allowed characters in username

                })
                    .AddRoles<IdentityRole>() // Add role
                    .AddEntityFrameworkStores<AppDBContext>();


                var jwtKey = secretKey;
                var jwtIssuer = issuer;
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtIssuer,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                        };
                    });


                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "RVMS Service",
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
                });


                var app = builder.Build();

                //app.MapIdentityApi<IdentityUser>();
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

                // Add this seed data method before app.Run()
                using (var scope = app.Services.CreateScope())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    // Create Admin role if it doesn't exist
                    if (!await roleManager.RoleExistsAsync("Admin"))
                    {
                        await roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                // Create Supervisor role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Supervisor"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Supervisor"));
                }

                // Create Operator role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Operator"))
                    {
                        await roleManager.CreateAsync(new IdentityRole("Operator"));
                    }

                    //checking if there is user in the database
                    bool hasAnyUser = await userManager.Users.AnyAsync();
                    if (!hasAnyUser)
                    {
                        // Create default admin user if it doesn't exist
                        var adminUser = await userManager.FindByNameAsync("admin");
                        if (adminUser == null)
                        {
                            var admin = new ApplicationUser
                            {
                                UserName = "admin",
                                Email = "admin@example.com",
                                EmailConfirmed = true,
                                FullName = "System Administrator"
                            };

                            var result = await userManager.CreateAsync(admin, "Admin1!");
                            if (result.Succeeded)
                            {
                                // Assign Admin role to the admin user
                                await userManager.AddToRoleAsync(admin, "Admin");
                            }
                        }
                    }


                }


                app.Logger.LogInformation("Starting web host");
            app.Run();

            }
            //catch (Exception ex)
            //{
            //    Log.Fatal(ex, "Host terminated unexpectedly");
            //}
            //finally
            //{
            //    Log.CloseAndFlush();

            //}
        //}
    }
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


