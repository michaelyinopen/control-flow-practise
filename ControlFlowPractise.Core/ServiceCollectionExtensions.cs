using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ControlFlowPractise.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Consumer has to register BudgetDataDbContext, ComprehensiveDataDbContext and IExternalPartyProxy
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddWarrantyService(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<ValidatableRequestValidator>(ServiceLifetime.Singleton);

            services.AddSingleton<FailureClassification>();
            services.AddScoped<BudgetDataWrapper>();
            services.AddScoped<ComprehensiveDataWrapper>();
            services.AddSingleton<RequestValidator>();
            services.AddSingleton<RequestBuilder>();
            services.AddScoped<ExternalPartyWrapper>();
            services.AddSingleton<ResponseValidator>();
            services.AddSingleton<ResponseConverter>();

            services.AddScoped<IWarrantyService, WarrantyService>();

            return services;
        }
    }
}
