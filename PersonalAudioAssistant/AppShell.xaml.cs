using PersonalAudioAssistant.Views;
using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.Views.Users;
using PersonalAudioAssistant.Views.History;

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
            Routing.RegisterRoute(nameof(UsersListPage), typeof(UsersListPage));
            Routing.RegisterRoute(nameof(AddUserPage), typeof(AddUserPage));
            Routing.RegisterRoute(nameof(UpdateUserPage), typeof(UpdateUserPage));
            Routing.RegisterRoute(nameof(AnaliticsPage), typeof(AnaliticsPage));
            Routing.RegisterRoute(nameof(PaymentPage), typeof(PaymentPage));
            Routing.RegisterRoute(nameof(GetAccessToHistoryModalPage), typeof(GetAccessToHistoryModalPage));
            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
            Routing.RegisterRoute(nameof(MessagesPage), typeof(MessagesPage));
        }

        private async void SignOut_Clicked(object sender, EventArgs e)
        {
            this.ShowPopup(new MenuPage());
        }
    }
}