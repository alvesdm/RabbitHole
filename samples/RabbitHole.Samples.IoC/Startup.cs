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

            services.AddScoped<IDebitBus>(s => new DebitBus(Factories.ClientFactory.Create()));
            //services.AddScoped(s => new DebitBus(Factories.ClientFactory.Create()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }

    public interface IDebitBus : IBus
    {
        void Publish(DebitAccountCommand message);
    }
    
    public class DebitBus : BusBase, IDebitBus
    {
        public DebitBus(IClient client) : base(client){ }

        public void Publish(DebitAccountCommand message) {
            Client
                .DeclareExchange(e=>e.WithName(typeof(DebitAccountCommand).Name))
                .Publish<DebitAccountCommand>(m=>m.WithExchange(typeof(DebitAccountCommand).Name).WithMessage(message));
        }
    }

    public class DebitAccountCommand : IMessage
    {
        public string AccountNumber { get; set; }
        public decimal Amouunt { get; set; }
    }
}
