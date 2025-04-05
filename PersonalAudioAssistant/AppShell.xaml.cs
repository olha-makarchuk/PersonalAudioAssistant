using PersonalAudioAssistant.Views;
using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.Views.Users;

namespace PersonalAudioAssistant
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = this;

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AuthorizationPage), typeof(AuthorizationPage));
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(ProgramPage), typeof(ProgramPage));
            Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(CloneVoicePage), typeof(CloneVoicePage));
            Routing.RegisterRoute(nameof(UsersListPage), typeof(UsersListPage));
            Routing.RegisterRoute(nameof(AddUserPage), typeof(AddUserPage));
            Routing.RegisterRoute(nameof(UpdateUserPage), typeof(UpdateUserPage));
            Routing.RegisterRoute(nameof(AnaliticsPage), typeof(AnaliticsPage));
        }

        private void SignOut_Clicked(object sender, EventArgs e)
        {
            this.ShowPopup(new MenuPage());
        }
    }
}