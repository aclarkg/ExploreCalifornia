using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ExploreCalifornia.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ExploreCalifornia
{
    public class Startup
    {
        private readonly IConfigurationRoot configuration;

        public Startup(IHostingEnvironment env) // Constructor
        {
            configuration = new ConfigurationBuilder()
                                      .AddEnvironmentVariables()
                                      .AddJsonFile(env.ContentRootPath + "/config.json")
                                      .AddJsonFile(env.ContentRootPath + "/config.development.json", true) // Optional file
                                      .Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddTransient<SpecialsDataContext>();

            services.AddTransient<FormattingService>();

            services.AddTransient<FeatureToggles>(x => new FeatureToggles
            {
                EnableDeveloperExceptions =
                 configuration.GetValue<bool>("FeatureToggles:EnableDeveloperExceptions")
            });

            services.AddDbContext<BlogDataContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("BlogDataContext");
                options.UseSqlServer(connectionString);
            });

            services.AddDbContext<SpecialsDataContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("SpecialsDataContext");
                options.UseSqlServer(connectionString);
            });

            services.AddDbContext<IdentityDataContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("IdentityDataContext");
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDataContext>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
                                    ILoggerFactory loggerFactory,
                                    FeatureToggles features
                                    )
        {
            app.UseExceptionHandler("/error.html");

            //var configuration = new ConfigurationBuilder()
            //                        .AddEnvironmentVariables()
            //                        .AddJsonFile(env.ContentRootPath + "/config.json")
            //                        .AddJsonFile(env.ContentRootPath + "/config.development.json", true) // Optional file
            //                        .Build();

            //if (env.IsDevelopment())
            //if (configuration["EnableDeveloperExceptions"] == "True")
            //if (configuration.GetValue<bool>("FeatureToggles:EnableDeveloperExceptions"))
            if(features.EnableDeveloperExceptions)
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Contains("invalid"))
                    throw new Exception("ERROR!");

                await next();
            });

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});

            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute("Default",
                    "{controller=Home}/{action=Index}/{id?}"
                    );
            });

            app.UseFileServer();
        }
    }
}
