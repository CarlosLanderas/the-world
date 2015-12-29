using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Dnx.Runtime;
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

    public Startup(IApplicationEnvironment appEnv)
    {
      var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
        .AddJsonFile("config.json")
        .AddEnvironmentVariables();

      Configuration = builder.Build();
    }

    // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvc().AddJsonOptions(opt =>
      {
          opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
      });

      services.AddLogging();

      services.AddEntityFramework()
        .AddSqlServer()
        .AddDbContext<WorldContext>();

      services.AddScoped<CoordService>();
      services.AddTransient<WorldContextSeedData>();
      services.AddScoped<IWorldRepository, WorldRepository>();
        services.AddScoped<ICoordService, CoordService>();

#if DEBUG
      services.AddScoped<IMailService, DebugMailService>();
#else
      services.AddScoped<IMailService, MailService>();
#endif
    }

    public void Configure(IApplicationBuilder app, WorldContextSeedData seeder, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddDebug(LogLevel.Warning);
      
      app.UseStaticFiles();

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

      seeder.EnsureSeedData();
    }
  }
}
