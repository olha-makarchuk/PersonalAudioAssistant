using AndroidX.Lifecycle;
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

    public MediaElement MediaElement => mediaElement;
}