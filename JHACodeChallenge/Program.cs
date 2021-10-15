using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

namespace JHACodeChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();


            // call the run method in App
            serviceProvider.GetService<App>().Run();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// register services here.
        /// </summary>
        /// <returns>IServiceCollection</returns>
        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            var config = LoadConfiguration();
            services.AddSingleton(config);                              // register configuration
            services.AddMemoryCache();
            services.AddSingleton<ICacheMemory, CashMemory>();
            services.AddSingleton<ITweetTrack, TweetTrack>();
            services.AddSingleton<ITwitterServices, TwitterServices>();
            services.AddSingleton<IReporting, Reporting>();
            services.AddTransient<App>();                               // register app, required to run the application

            return services;
        }

        /// <summary>
        /// Load Configurations
        /// </summary>
        /// <returns>IConfiguration</returns>
        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
    }
}
