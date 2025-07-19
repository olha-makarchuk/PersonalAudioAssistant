using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PeronalAudioAssistant.Application.PlatformFeatures
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        }
    }
}
