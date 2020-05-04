using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxy.Core.EventBus.Abstractions;
using Galaxy.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQEventBusSample.Events;
using RabbitMQEventBusSample.Handlers;

namespace RabbitMQEventBusSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
            services.AddGalaxy(option => { })
                .WithRabbitEventBus(opt =>
                {
                    opt.Host = "localhost";
                    opt.VirtualHost = "librahost";
                    opt.ExchangeName = "libra.exchange.direct.sample";
                    opt.ExchangeType = "direct";
                    opt.Username = "admin";
                    opt.Password = "huatek@123";
                    opt.QueueName = "libra.queue.sample";
                });
            services.AddSingleton(typeof(IRepository<,>), typeof(Repository<,>));
            EventBusConfiguration.ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.BuildGalaxy();
            EventBusConfiguration.Configure(app);
        }
    }
}
