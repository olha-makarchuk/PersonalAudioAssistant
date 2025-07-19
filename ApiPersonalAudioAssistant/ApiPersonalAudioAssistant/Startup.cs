using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands;
using ApiPersonalAudioAssistant.Application.Services;
using ApiPersonalAudioAssistant.Persistence.Context;
using ApiPersonalAudioAssistant.Persistence.Repositories;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc.Versioning;
using PeronalAudioAssistant.Application.PlatformFeatures;

namespace ApiPersonalAudioAssistant
{
    public class Startup
    {
        public readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile($"appsettings.json", false, true)
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpLogging(o => o = new HttpLoggingOptions());
            services.AddEndpointsApiExplorer();
            services.AddLogging();
            services.AddDefaultCorrelationId();
            services.AddSwaggerGen();
            services.AddApiVersioning();
            services.AddApplication();
            services.AddApiVersioning(t =>
            {
                t.ApiVersionReader = new UrlSegmentApiVersionReader();
                t.ReportApiVersions = true;
            });

              services.AddScoped<PasswordManager>();

            services.AddScoped<TokenBase>();
            services.AddScoped<PasswordManager>();
            services.AddScoped<IApiClient, ApiClientVoiceEmbedding>();
            services.AddScoped<ApiClientVoiceEmbedding>();
            services.AddScoped<IMainUserRepository, MainUserRepository>();
            services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
            services.AddScoped<ISubUserRepository, SubUserRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IAutoPaymentRepository, AutoPaymentRepository>();
            services.AddScoped<IPaymentHistoryRepository, PaymentHistoryRepository>();
            services.AddScoped<IVoiceRepository, VoiceRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IMoneyUsedRepository, MoneyUsedRepository>();
            services.AddScoped<IMoneyUsersUsedRepository, MoneyUsersUsedRepository>();

            services.AddScoped<IBlobStorage, BlobStorage>();
            services.AddSingleton<BlobStorageConfig>();

            ConfigureDb(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            //app.UseMiddleware<GlobalErrorHandlingMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        protected virtual void ConfigureDb(IServiceCollection services)
        {
            services.AddCosmos<CosmosDbContext>(_configuration.GetConnectionString("CosmosConnection")!,
                "AudioAssistantDB"); 
        }
    }
}
