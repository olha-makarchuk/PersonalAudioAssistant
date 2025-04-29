using PersonalAudioAssistant.Contracts.SubUser;
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
            await viewModel.LoadUsersAsync();
        }
    }
    /*
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is UsersListViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }*/
}
