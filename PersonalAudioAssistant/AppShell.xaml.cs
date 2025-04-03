using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.View;
using System;
using Microsoft.Maui.Dispatching;

namespace PersonalAudioAssistant
{
    public partial class AppShell : Shell
    {
        public AppShell(AuthTokenManager authTokenManager)
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AuthorizationPage), typeof(AuthorizationPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(ProgramPage), typeof(ProgramPage));

            authTokenManager.UserSignInStatusChanged += OnUserSignInStatusChanged;
        }

        private void OnUserSignInStatusChanged(object sender, bool isSignedIn)
        {
            // Оновлюємо властивість IsLogged на головному потоці
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsLogged = isSignedIn;
            });
        }

        public bool IsLogged
        {
            get => (bool)GetValue(IsLoggedProperty);
            set => SetValue(IsLoggedProperty, value);
        }

        public static readonly BindableProperty IsLoggedProperty =
            BindableProperty.Create(nameof(IsLogged), typeof(bool), typeof(AppShell), false);

    }
}
