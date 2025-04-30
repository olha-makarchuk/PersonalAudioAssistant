using PersonalAudioAssistant.ViewModel;
using PersonalAudioAssistant.ViewModel.Users;

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

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is PaymentViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }
}