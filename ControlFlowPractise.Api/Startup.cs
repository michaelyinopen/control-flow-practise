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
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
                options.UseSqlServer(
                    Configuration.GetConnectionString("BudgetDataDb"),
                    providerOptions => providerOptions.EnableRetryOnFailure()));
            services.AddDbContextPool<ComprehensiveDataDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("ComprehensiveDataDb"),
                    providerOptions => providerOptions.EnableRetryOnFailure()));

            services.AddScoped<IExternalPartyProxy, ExternalPartyProxy>();

            services.AddWarrantyService();

            services
                .AddControllers(options =>
                {
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
                })
                .AddNewtonsoftJson(); ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            BudgetDataDbContext budgetDataDbContext,
            ComprehensiveDataDbContext comprehensiveDataDbContext)
        {
            // creates or apply migrations to the databases
            // remove this if there is an alternative migration strategy
            budgetDataDbContext.Database.Migrate();
            comprehensiveDataDbContext.Database.Migrate();

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
