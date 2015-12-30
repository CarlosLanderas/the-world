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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
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
            var builder = new ConfigurationBuilder()
              .SetBasePath(appEnv.ApplicationBasePath)
              .AddJsonFile("config.json")
              .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSession();
            services.AddCaching();
            services.AddLogging();
            services.AddIdentity<WorldUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.Password.RequiredLength = 8;
            })
             .AddEntityFrameworkStores<WorldContext>();


            services.AddEntityFramework()
              .AddSqlServer()
              .AddDbContext<WorldContext>();

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
            app.UseStaticFiles();
            app.UseSession();
            app.UseIdentity();

            app.UseCookieAuthentication(config =>
            {
                config.LoginPath = "/Auth/Login";
                config.CookieName = "TheWorldAuth";
                config.AutomaticAuthenticate = true;
                config.AutomaticChallenge = true;
                config.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return Task.FromResult<object>(null);
                        }
                        else
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.FromResult<object>(null);
                        }
                    }
                };
            });

            if (_hostingEnvironment.IsDevelopment())
            {
                loggerFactory.AddDebug(LogLevel.Information);
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage(config =>
                {
                    config.EnableAll();
                });
            }
            else
            {
                loggerFactory.AddDebug(LogLevel.Debug);
            }
            

            app.UseMvc(config =>
            {
                config.MapRoute(
                    name: "Default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "App", action = "Index" }
                    );
            });

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            await seeder.EnsureSeedDataAsync();

            Mapper.Initialize(config =>
            {
                config.CreateMap<TripViewModel, Trip>().ReverseMap();
                config.CreateMap<StopViewModel, Stop>().ReverseMap();
            });
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
