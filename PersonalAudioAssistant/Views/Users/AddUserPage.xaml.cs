using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views.Users;

public partial class AddUserPage : ContentPage
{
	public AddUserPage(AddUserViewModel viewModel)
	{
		InitializeComponent();
        Shell.SetTitleView(this, null);
        BindingContext = viewModel;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is AddUserViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AddUserViewModel viewModel)
        {
            await viewModel.LoadVoicesAsync();
        }
    }

    public MediaElement MediaElement => mediaElement;
}