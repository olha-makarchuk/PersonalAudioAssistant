using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider services)
        {
            InitializeComponent();
            Services = services;

            // Ручне створення AppShell з отриманням AuthTokenManager з DI
            var authTokenManager = Services.GetService<AuthTokenManager>();
            MainPage = new AppShell(authTokenManager);
        }


    }

}
