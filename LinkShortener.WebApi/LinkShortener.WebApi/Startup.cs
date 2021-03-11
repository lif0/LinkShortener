using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace LinkShortener.WebApi
{
    public class Startup
    {
        private static string mongoDbConnStr = "mongodb://192.168.99.100:27017/compressed_link_db";
        private static string MongoСompressedLinkCollection = "compressed_links";
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mongoUrl = new MongoUrl(mongoDbConnStr);
            var mongoDb = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
            
            services.AddSingleton(mongoDb);
            services.AddScoped(_ => { return mongoDb.GetCollection<CompressedLinkEntity>(MongoСompressedLinkCollection); });
            services.AddRouting();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "LinkShortener WebApi", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LinkShortener WebApi"));
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}