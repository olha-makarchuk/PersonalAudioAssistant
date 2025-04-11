using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class PaymentPage : ContentPage
{
    public PaymentPage(PaymentViewModel viewModel)
    {
        InitializeComponent();
        Shell.SetTitleView(this, null);
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((PaymentViewModel)BindingContext).InitializeAsync();
    }
}