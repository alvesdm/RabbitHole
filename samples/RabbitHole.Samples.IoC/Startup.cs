using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RabbitHole.Samples.IoC
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddScoped(typeof(IDebitBus), (s) => {
                return RabbitHole.Factories.ClientFactory.Create();
            });

            services.AddTransient<DebitBus, DebitBus>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }

    public interface IDebitBus : IClient{}

    public class DebitBus
    {
        IClient _client;
        public DebitBus(IDebitBus client)
        {
            _client = client;
        }

        public void Publish(DebitAccountCommand message) {
            _client
                .WithExchange(e=>e.WithName(typeof(DebitAccountCommand).Name))
                .Publish<DebitAccountCommand>(m=>m.WithMessage(message));
        }
    }

    public class DebitAccountCommand : IMessage
    {
        public string AccountNumber { get; set; }
        public decimal Amouunt { get; set; }
    }
}
