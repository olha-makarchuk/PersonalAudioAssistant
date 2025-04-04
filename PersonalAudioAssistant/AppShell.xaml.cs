using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views;
using System;
using Microsoft.Maui.Dispatching;
using System.Globalization;
using CommunityToolkit.Maui.Views;

namespace PersonalAudioAssistant
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = this;

            Routing.RegisterRoute(nameof(AuthorizationPage), typeof(AuthorizationPage));
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(ProgramPage), typeof(ProgramPage));
            Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        }

        private void SignOut_Clicked(object sender, EventArgs e)
        {
            this.ShowPopup(new MenuPage());
        }
    }
}