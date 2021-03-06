using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlFlowPractise.BudgetData;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.ExternalParty;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static ControlFlowPractise.Core.ServiceCollectionExtensions;

namespace ControlFlowPractise.Api
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
            services.AddDbContextPool<BudgetDataDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("BudgetDataDb")));
            services.AddDbContextPool<ComprehensiveDataDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ComprehensiveDataDb")));

            services.AddScoped<IExternalPartyProxy, ExternalPartyProxy>();

            services.AddWarrantyService();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
