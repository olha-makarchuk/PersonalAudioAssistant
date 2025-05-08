using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class AnaliticsPage : ContentPage
{
    private readonly AnaliticsViewModel _vm;

    public AnaliticsPage(AnaliticsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_vm.IsBusy && _vm.UserMoneyPieChart == null)
        {
            _vm.IsBusy = true;
            try
            {
                await _vm.InitializeAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
            finally
            {
                _vm.IsBusy = false;
            }
        }
    }
}