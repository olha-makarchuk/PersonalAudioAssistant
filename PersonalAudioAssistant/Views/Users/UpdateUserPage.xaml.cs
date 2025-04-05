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
}