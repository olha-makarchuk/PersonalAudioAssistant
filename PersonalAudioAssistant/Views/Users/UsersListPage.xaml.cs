using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views.Users;

public partial class UsersListPage : ContentPage
{
    public UsersListPage(UsersListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UsersListViewModel viewModel)
        {
            await viewModel.LoadUsers();
        }
    }
}
