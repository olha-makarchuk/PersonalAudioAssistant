using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.View;
using System;
using Microsoft.Maui.Dispatching;
using System.Globalization;

namespace PersonalAudioAssistant
{
    public partial class AppShell : Shell
    {
        private readonly AuthTokenManager _authTokenManager;
        public AppShell(AuthTokenManager authTokenManager)
        {
            InitializeComponent();
            BindingContext = this;

            Routing.RegisterRoute(nameof(AuthorizationPage), typeof(AuthorizationPage));
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(ProgramPage), typeof(ProgramPage));

            _authTokenManager = authTokenManager;
            authTokenManager.UserSignInStatusChanged += OnUserSignInStatusChanged;
        }

        private async void OnFooterTapped(object sender, System.EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(View.AuthorizationPage));
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
        public bool IsLoggedInverse => !IsLogged;


        public static readonly BindableProperty IsLoggedProperty =
            BindableProperty.Create(nameof(IsLogged), typeof(bool), typeof(AppShell), false);

        public async Task Sign_Out(object sender, System.EventArgs e)
        {
            await _authTokenManager.SignOutAsync();
            await Shell.Current.GoToAsync("//AuthorizationPage");
        }
    }
}