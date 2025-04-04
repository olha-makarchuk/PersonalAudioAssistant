using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views.Users;

public partial class UpdateUserPage : ContentPage
{
	public UpdateUserPage(UpdateUserViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}