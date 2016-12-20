using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using Wicture.DbRESTFul;
using Wicture.DbRESTFul.Auth;
using Wicture.DbRESTFul.Gateway;
using Wicture.MicroService.Middlewares;
using Wicture.MicroService.Models;

namespace Wicture.MicroService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            
            var logMinLevel = env.IsDevelopment() 
                            ? LogEventLevel.Verbose
                            : (LogEventLevel)Enum.Parse(typeof(LogEventLevel), Configuration.GetSection("Logging:LogLevel:Default").Value);

            Serilog.Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.ControlledBy(new LoggingLevelSwitch {MinimumLevel = logMinLevel})
                                .WriteTo.RollingFile(Path.Combine(env.ContentRootPath + @"/log/", "log-{Date}.txt"))
                                .CreateLogger();
        }

        public IConfigurationRoot Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => { options.Filters.Add(new ApiExceptionFilterAttribute(new LogHelper())); });

            services.Configure<ConsulConfig>(config => Configuration.GetSection("consulConfig").Bind(config));
            services.AddSingleton<IConsulClient, ConsulClient>(p =>new ConsulClient(consulConfig =>
            {
                consulConfig.Address =
                    new Uri(Configuration["CONSUL_SERVER_ADDRESS"] ?? Configuration["consulConfig:address"]);
            }));            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();

            ConfigurationManager.Setup(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), loggerFactory, env.IsDevelopment());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseMvc();
            }

            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseHealthEndpoint();
            app.UseEnvironmentEndpoint();
            app.UseInfoEndpoint();

            JwtBearerTokenProvider provider = new JwtBearerTokenProvider();
            var filter = new JwtTokenAuthorizationFilter(provider.Setup(app))
            {
                ReturnDefaultUserIfFault = false,
                DbGateway = new DefaultMySQLGateway()
            };

            app.UseMicroService(Configuration, filter);
        }
    }
}