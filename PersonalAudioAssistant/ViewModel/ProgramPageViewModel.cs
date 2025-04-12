using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using System.Globalization;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class ProgramPageViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly ITextToSpeech textToSpeech;
        private readonly ISpeechToText speechToText;
        private readonly ManageCacheData _manageCacheData;

        [ObservableProperty]
        private List<Locale>? locales;

        [ObservableProperty]
        private Locale? locale;

        [ObservableProperty]
        private string? recognitionText;

        public ProgramPageViewModel(ITextToSpeech textToSpeech, ISpeechToText speechToText, IMediator mediator, ManageCacheData manageCacheData)
        {
            this.textToSpeech = textToSpeech;
            this.speechToText = speechToText;
            _manageCacheData = manageCacheData;
            _mediator = mediator;
            Locales = new();
            SetLocalesCommand.Execute(null);
        }

        [RelayCommand]
        async Task SetLocales()
        {
            Locales = (await textToSpeech.GetLocalesAsync()).ToList();
            Locale = Locales.FirstOrDefault();
        }

        [RelayCommand(IncludeCancelCommand = true)]
        async Task Listen(CancellationToken cancellationToken)
        {
            var isAuthorized = await speechToText.RequestPermissions();
            var usersList = await _manageCacheData.GetUsersAsync();

            if (isAuthorized)
            {
                try
                {
                    RecognitionText = string.Empty;
                    // Отримання остаточного результату розпізнавання
                    RecognitionText = await speechToText.Listen(
                        CultureInfo.GetCultureInfo(Locale?.Language ?? "en-us"),
                        new Progress<string>(partialText =>
                        {
                            RecognitionText += partialText + " ";
                        }), usersList, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Скасування очікуване, тому не показуємо помилку.
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show(cancellationToken);
                }
            }
            else
            {
                await Toast.Make("Permission denied").Show(cancellationToken);
            }
        }
    }
}
