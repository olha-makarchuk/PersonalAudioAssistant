using AndroidX.Lifecycle;
using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views.Users;

public partial class AddUserPage : ContentPage
{
	public AddUserPage(AddUserViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}