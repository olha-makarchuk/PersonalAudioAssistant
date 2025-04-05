using AndroidX.Lifecycle;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class AnaliticsPage : ContentPage
{
	public AnaliticsPage(AnaliticsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}