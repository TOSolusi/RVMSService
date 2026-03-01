
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
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
            //string connectionString = $"Server={SettingsConfig["Server"].ToString()};Database={SettingsConfig["Database"].ToString()}; Integrated Security={SettingsConfig["IntegratedSecurity"].ToString()}; Encrypt=false;";    //SettingsConfig["ConnectionString"]?.ToString() ?? "";
            //string secretKey = SettingsConfig["JwtKey"]?.ToString();
            //string issuer = SettingsConfig["JwtIssuer"]?.ToString();

            // Build connection string supporting both Windows and SQL Authentication
            bool useIntegratedSecurity = string.Equals(
                SettingsConfig["IntegratedSecurity"]?.ToString(), "true", StringComparison.OrdinalIgnoreCase);

            string connectionString;
            if (useIntegratedSecurity)
            {
                connectionString = $"Server={SettingsConfig["Server"]};" +
                                   $"Database={SettingsConfig["Database"]};" +
                                   $"Integrated Security=True;Encrypt=false;";
            }
            else
            {
                connectionString = $"Server={SettingsConfig["Server"]};" +
                                   $"Database={SettingsConfig["Database"]};" +
                                   $"User ID={SettingsConfig["UserID"]};" +
                                   $"Password={SettingsConfig["SqlPassword"]};" +
                                   $"Integrated Security=False;Encrypt=false;" +
                                   $"TrustServerCertificate=True;";
            }

            string secretKey = SettingsConfig["JwtKey"]?.ToString();
            string issuer = SettingsConfig["JwtIssuer"]?.ToString();

            //// ── Installer setup mode: test connection, create DB, apply migrations, exit ──
            //if (args.Contains("--setup"))
            //{
            //    await RunDatabaseSetup(connectionString, useIntegratedSecurity);
            //    return;
            //}


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
            builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
            builder.Services.AddScoped<IGateService, GateService>();
            builder.Services.AddScoped<IDestinationService, DestinationService>();
            builder.Services.AddScoped<IVisitTypeService, VisitTypeService>();
            builder.Services.AddScoped<IQRCodeService, QRCodeService>();
            builder.Services.AddScoped<IVisitorService, VisitorService>();
            builder.Services.AddScoped<IVisitService, VisitService>();
            builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
            builder.Services.AddSingleton<ITelegramService, TelegramService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();
            builder.Services.AddHostedService<TelegramBotPollingService>();

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

            // 2. ADD the Setup & Seed block here, right after app.Build()
            if (args.Contains("--setup"))
            {
                // First, create the database and run migrations
                await RunDatabaseSetup(connectionString, useIntegratedSecurity);

                // Then, run the seed logic using the full DI container
                using (var scope = app.Services.CreateScope())
                {
                    // Notice we don't need MigrateAsync here because RunDatabaseSetup already did it
                    await SeedDataAsync(scope.ServiceProvider);
                }

                Console.WriteLine("Setup and Seeding completed successfully.");
                return; // Exit setup mode safely
            }

            // 3. For normal service startup, ensure tables exist before seeding
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();

                    // Automatically apply any pending migrations (creates tables like AspNetRoles)
                    await dbContext.Database.MigrateAsync();

                    // Now it's safe to seed data
                    await SeedDataAsync(scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                }
            }

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

            //// Add this seed data method before app.Run()
            //using (var scope = app.Services.CreateScope())
            //{
            //    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //    // Create Admin role if it doesn't exist
            //    if (!await roleManager.RoleExistsAsync("Admin"))
            //    {
            //        await roleManager.CreateAsync(new IdentityRole("Admin"));
            //    }

            //    // Create Supervisor role if it doesn't exist
            //    if (!await roleManager.RoleExistsAsync("Supervisor"))
            //    {
            //        await roleManager.CreateAsync(new IdentityRole("Supervisor"));
            //    }

            //    // Create Operator role if it doesn't exist
            //    if (!await roleManager.RoleExistsAsync("Operator"))
            //    {
            //        await roleManager.CreateAsync(new IdentityRole("Operator"));
            //    }

            //    //checking if there is user in the database
            //    bool hasAnyUser = await userManager.Users.AnyAsync();
            //    if (!hasAnyUser)
            //    {
            //        // Create default admin user if it doesn't exist
            //        var adminUser = await userManager.FindByNameAsync("admin");
            //        if (adminUser == null)
            //        {
            //            var admin = new ApplicationUser
            //            {
            //                UserName = "admin",
            //                Email = "admin@example.com",
            //                EmailConfirmed = true,
            //                FullName = "System Administrator"
            //            };

            //            var result = await userManager.CreateAsync(admin, "Admin1!");
            //            if (result.Succeeded)
            //            {
            //                // Assign Admin role to the admin user
            //                await userManager.AddToRoleAsync(admin, "Admin");
            //            }
            //        }
            //    }

            //    //Create Initial Gate
            //    var gateService = scope.ServiceProvider.GetRequiredService<IGateService>();
                
            //    bool hasAnyGate = (await gateService.GetAllGates()).Any();
            //    if (!hasAnyGate)
            //    {
            //        var initialGate = new GateModel { GateName = "Main Office", Description = "Main Office in Main Building", Status = true, UpdatedAt = DateTime.Now };
            //        await gateService.AddGate(initialGate);
            //    }

                    

            //}


            app.Logger.LogInformation("Starting web host");
            app.Run();

        }

        /// <summary>
        /// Called by installer with --setup flag.
        /// 1. Tests SQL Server connectivity
        /// 2. Creates the database if it doesn't exist (Windows Auth only)
        /// 3. Applies all EF Core migrations
        /// </summary>
        private static async Task RunDatabaseSetup(string connectionString, bool useIntegratedSecurity)
        {
            Console.WriteLine("=== RVMS Database Setup ===");

            var csBuilder = new SqlConnectionStringBuilder(connectionString);
            var dbName = csBuilder.InitialCatalog;

            // ── Step 1: Test SQL Server connectivity ──
            Console.WriteLine("Step 1: Testing SQL Server connectivity...");
            var masterCsBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master",
                ConnectTimeout = 10
            };

            try
            {
                using var testConn = new SqlConnection(masterCsBuilder.ConnectionString);
                await testConn.OpenAsync();
                Console.WriteLine($"  Connected to SQL Server: {csBuilder.DataSource}");
                Console.WriteLine($"  Auth mode: {(useIntegratedSecurity ? "Windows (Integrated)" : "SQL Authentication")}");
                testConn.Close();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"  FAILED: Cannot connect to SQL Server at '{csBuilder.DataSource}'");
                Console.WriteLine($"  Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("  Possible causes:");
                Console.WriteLine("    - SQL Server is not running");
                Console.WriteLine("    - Server name is incorrect");
                Console.WriteLine("    - SQL Server does not allow remote connections");
                if (!useIntegratedSecurity)
                    Console.WriteLine("    - SQL login credentials are incorrect");
                else
                    Console.WriteLine("    - The current Windows account does not have SQL Server access");
                Console.WriteLine();
                Console.WriteLine("  Setup aborted. Fix the connection and run again.");
                Environment.ExitCode = 1;
                return;
            }

            // ── Step 2: Create database if it doesn't exist ──
            Console.WriteLine($"Step 2: Checking database '{dbName}'...");
            try
            {
                using var masterConn = new SqlConnection(masterCsBuilder.ConnectionString);
                await masterConn.OpenAsync();

                using var checkCmd = masterConn.CreateCommand();
                checkCmd.CommandText = "SELECT DB_ID(@dbname)";
                checkCmd.Parameters.AddWithValue("@dbname", dbName);
                var result = await checkCmd.ExecuteScalarAsync();

                if (result is null or DBNull)
                {
                    Console.WriteLine($"  Database '{dbName}' not found. Creating...");
                    using var createCmd = masterConn.CreateCommand();
                    createCmd.CommandText = $"CREATE DATABASE [{dbName}]";
                    await createCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"  Database '{dbName}' created successfully.");
                    Console.WriteLine("  Waiting for database to come online...");
                    await Task.Delay(3000);
                }
                else
                {
                    Console.WriteLine($"  Database '{dbName}' already exists.");
                }

                masterConn.Close();
            }
            catch (SqlException ex) when (ex.Number == 262 || ex.Number == 911)
            {
                // 262 = CREATE DATABASE permission denied
                // 911 = Database does not exist (no access to master)
                Console.WriteLine($"  WARNING: No permission to create database. {ex.Message}");
                Console.WriteLine("  Ensure the database was created by the installer script.");
                Console.WriteLine("  Attempting to continue with migrations...");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"  WARNING: Could not create database. {ex.Message}");
                Console.WriteLine("  Attempting to continue with migrations...");
            }

            // ── Step 3: Verify connection to application database ──
            Console.WriteLine($"Step 3: Verifying connection to '{dbName}'...");
            try
            {
                using var dbConn = new SqlConnection(connectionString);
                await dbConn.OpenAsync();
                Console.WriteLine($"  Connected to '{dbName}' successfully.");
                dbConn.Close();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"  FAILED: Cannot connect to database '{dbName}'.");
                Console.WriteLine($"  Error: {ex.Message}");
                Console.WriteLine();
                if (!useIntegratedSecurity)
                    Console.WriteLine($"  Ensure login '{csBuilder.UserID}' has access to '{dbName}'.");
                else
                    Console.WriteLine("  Ensure the Windows service account has access to the database.");
                Environment.ExitCode = 1;
                return;
            }

            // ── Step 4: Apply EF Core migrations ──
            Console.WriteLine("Step 4: Applying EF Core migrations...");
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDBContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new AppDBContext(optionsBuilder.Options);

                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Count > 0)
                {
                    Console.WriteLine($"  Found {pendingMigrations.Count} pending migration(s):");
                    foreach (var m in pendingMigrations)
                        Console.WriteLine($"    - {m}");

                    await context.Database.MigrateAsync();
                    Console.WriteLine("  All migrations applied successfully.");
                }
                else
                {
                    Console.WriteLine("  Database schema is up to date. No migrations needed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  FAILED: Migration error — {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine();
            Console.WriteLine("=== RVMS Database Setup Completed Successfully ===");
        }


        private static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create Admin role
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            // Create Supervisor role
            if (!await roleManager.RoleExistsAsync("Supervisor"))
                await roleManager.CreateAsync(new IdentityRole("Supervisor"));

            // Create Operator role
            if (!await roleManager.RoleExistsAsync("Operator"))
                await roleManager.CreateAsync(new IdentityRole("Operator"));

            bool hasAnyUser = await userManager.Users.AnyAsync();
            if (!hasAnyUser)
            {
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
                        await userManager.AddToRoleAsync(admin, "Admin");
                    }
                }
            }

            var gateService = serviceProvider.GetRequiredService<IGateService>();
            bool hasAnyGate = (await gateService.GetAllGates()).Any();
            if (!hasAnyGate)
            {
                var initialGate = new GateModel
                {
                    GateName = "Main Office",
                    Description = "Main Office in Main Building",
                    Status = true,
                    UpdatedAt = DateTime.Now
                };
                await gateService.AddGate(initialGate);
            }
        }

    }
}

