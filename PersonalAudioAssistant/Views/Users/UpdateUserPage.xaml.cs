using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views.Users;

public partial class UpdateUserPage : ContentPage
{
	public UpdateUserPage(UpdateUserViewModel viewModel)
	{
		InitializeComponent();
        Shell.SetTitleView(this, null);
        BindingContext = viewModel;
    }
    
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is UpdateUserViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }

    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UpdateUserViewModel viewModel)
        {
            await viewModel.LoadVoicesAsync();
        }
    }

    public MediaElement MediaElement => mediaElement;
}