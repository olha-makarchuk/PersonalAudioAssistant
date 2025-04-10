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

    public MediaElement MediaElement => mediaElement;
}