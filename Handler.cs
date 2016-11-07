using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using RimDev.Stuntman.Core;

public partial class Handler {

    public IConfigurationRoot Configuration { get; }
    private IHostingEnvironment Env;

    public Handler(IHostingEnvironment env)
    {
        Env = env;

        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables(prefix: "ASPNETCORE_");

        if (env.IsDevelopment())
        {
            // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
            // builder.AddUserSecrets();

            // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
            builder.AddApplicationInsightsSettings(developerMode: true);
        }

        Configuration = builder.Build();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add framework services.
        services.AddApplicationInsightsTelemetry(Configuration);

        // sqlite
        // services.AddDbContext<DB>(options => options.UseSqlite(Configuration.GetConnectionString("Sqlite")));

        // in-memory
        services.AddDbContext<DB>(options => options.UseInMemoryDatabase());

        // postgresql
        // Use a PostgreSQL database
        // string endpoint = Env.IsDevelopment() ? "Dev" : "Prod";
        // services.AddDbContext<DB>(options =>
        //     options.UseNpgsql(
        //         Configuration.GetConnectionString($"Postgres:{endpoint}")));

        // identity middleware -> sets up scoped services for UserManager<User> and SignInManager<User>
        // services.AddIdentity<User, NormalRole>()
        //         .AddEntityFrameworkStores<DB, int>()
        //         .AddDefaultTokenProviders();
        // services.AddScoped<IAuthService, AuthService>();
        // identity options
        // services.Configure<IdentityOptions>(options =>
        // {
        //     // Password settings
        //     options.Password.RequireDigit = true;
        //     options.Password.RequiredLength = 16;
        //     options.Password.RequireNonAlphanumeric = true;
        //     options.Password.RequireUppercase = true;
        //     options.Password.RequireLowercase = true;
            
        //     // Lockout settings
        //     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        //     options.Lockout.MaxFailedAccessAttempts = 10;
            
        //     // Cookie settings
        //     options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
        //     options.Cookies.ApplicationCookie.LoginPath = "/account/login";
        //     options.Cookies.ApplicationCookie.LogoutPath = "/account/logoff";
            
        //     // User settings
        //     options.User.RequireUniqueEmail = true;
        // });

        // messaging
        // services.AddTransient<IEmail, MessageService>();
        // services.AddTransient<ISms, MessageService>();
        
        // session middleware (comment out if using identity above)
        services.AddDistributedMemoryCache();
        services.AddSession(o => {
            o.IdleTimeout = TimeSpan.FromSeconds(120);
        });

        // CORS policy for JSON requests from other apps
        services.AddCors(options =>
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

        // MVC
        services.AddMvc(options =>
        {
            if(!Env.IsDevelopment()) {
                options.Filters.Add(new RequireHttpsAttribute());
            }
        });

        // Model repos
        // -----------
        // instead of
        //      services.AddScoped<IRepository<Card>, Repo<Card>>();
        //      services.AddScoped<IRepository<Blog>, Repo<Blog>>();
        //      etc...
        // do
        RegisterRepos(services); // RegisterRepos() is defined in Models.cs

        // Inject an implementation of ISwaggerProvider with defaulted settings applied
        services.AddSwaggerGen();

        services.ConfigureSwaggerGen(options =>
        {
            options.SingleApiVersion(new Info
            {
                Version = Configuration["Swagger:Version"],
                Title = Configuration["Swagger:Title"],
                Description = Configuration["Swagger:Description"]
            });
            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();
            options.DescribeAllEnumsAsStrings();
        });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger, DB db) {
        // logger.AddConsole(Configuration.GetSection("Logging"));
        logger.AddDebug();

        // if using session directly
        app.UseSession();

        // or if using identity (which includes session, too)
        // app.UseIdentity();
        // app.EnsureRolesCreated();

        // Stuntman (user-impersonation)
        // if(env.IsDevelopment()){
        //     CreateStuntUsers(); // defined in Models/Auth.cs
        //     app.UseStuntman(StuntmanOptions);
        // }

        // using CORS policy
        app.UseCors("CorsPolicy");

        // Example custom middleware
        // app.Use(async (context, next) =>
        // {
        //     await context.Response.WriteAsync("Pre Processing");
        //     await next();
        //     await context.Response.WriteAsync("Post Processing");
        // });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            app.UseStatusCodePages();
        }

        Seed.Initialize(db, env.IsDevelopment());

        // app.UseApplicationInsightsRequestTelemetry();
        // app.UseApplicationInsightsExceptionTelemetry();z

        app.UseStaticFiles();

        // facebook authentication w/ OAuth
        var facebookId = Configuration["Auth:Facebook:AppId"];
        var facebookSecret = Configuration["Auth:Facebook:AppSecret"];
        if (!string.IsNullOrWhiteSpace(facebookId) && !string.IsNullOrWhiteSpace(facebookSecret))
        {
            app.UseFacebookAuthentication(new FacebookOptions
            {
                AppId = facebookId,
                AppSecret = facebookSecret
            });
        }

        // google authentication w/ OAuth
        var googleId = Configuration["Auth:Google:ClientId"];
        var googleSecret = Configuration["Auth:Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
        {
            app.UseGoogleAuthentication(new GoogleOptions
            {
                ClientId = googleId,
                ClientSecret = googleSecret
            });
        }

        app.UseMvc();

        // Enable middleware to serve generated Swagger as a JSON endpoint and to show Swagger doc pages
        app.UseSwagger();
        app.UseSwaggerUi();
    }

}

