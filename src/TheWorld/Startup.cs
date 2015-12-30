using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Newtonsoft.Json.Serialization;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;
        private IHostingEnvironment _hostingEnvironment;
        public Startup(IApplicationEnvironment appEnv, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
              .AddJsonFile("config.json")
              .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                if (_hostingEnvironment.IsProduction())
                {
                   // config.Filters.Add(new RequireHttpsAttribute());
                }
            })
            .AddJsonOptions(opt =>
            {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddSession();
            services.AddCaching();
            services.AddLogging();
            services.AddIdentity<WorldUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<WorldContext>();

            services.ConfigureCookieAuthentication(config =>
            {
                config.LoginPath = "/Auth/Login"; 
                config.CookieName = "TheWorldAuth";
                config.Notifications = new CookieAuthenticationNotifications()
                {
                    OnApplyRedirect = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                };
            });

            
            services.AddEntityFramework()
              .AddSqlServer()
              .AddDbContext<WorldContext>();

            services.AddScoped<CoordService>();
            services.AddTransient<WorldContextSeedData>();
            services.AddScoped<IWorldRepository, WorldRepository>();
            services.AddScoped<ICoordService, CoordService>();
            services.AddScoped<IProfileService, DefaultProfileService>();
            services.AddSingleton<IMemoryCache, MemoryCache>();

#if DEBUG
            services.AddScoped<IMailService, DebugMailService>();
#else
    //  services.AddScoped<IMailService, MailService>();
#endif
        }

        public async void Configure(IApplicationBuilder app, WorldContextSeedData seeder, ILoggerFactory loggerFactory)
        {

            if (_hostingEnvironment.IsDevelopment())
            {
                loggerFactory.AddDebug(LogLevel.Information);
                app.UseErrorPage();
            }
            else
            {
                loggerFactory.AddDebug(LogLevel.Debug);
            }

            app.UseStaticFiles();
            app.UseSession();
            
            app.UseIdentity();

            Mapper.Initialize(config =>
            {
                config.CreateMap<TripViewModel, Trip>().ReverseMap();
                config.CreateMap<StopViewModel, Stop>().ReverseMap();
            });

            app.UseMvc(config =>
            {
                config.MapRoute(
            name: "Default",
            template: "{controller}/{action}/{id?}",
            defaults: new { controller = "App", action = "Index" }
            );
            });
            
            await seeder.EnsureSeedDataAsync();
        }
    }
}
