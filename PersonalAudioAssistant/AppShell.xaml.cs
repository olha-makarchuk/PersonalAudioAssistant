using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.View;
using System;

namespace PersonalAudioAssistant
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AuthorizationPage), typeof(AuthorizationPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(ProgramPage), typeof(ProgramPage));
        }
    }
}
