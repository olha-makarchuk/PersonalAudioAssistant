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
        private CancellationTokenSource? _listenCts;

        [ObservableProperty]
        private List<Locale>? locales;

        [ObservableProperty]
        private Locale? locale;

        [ObservableProperty]
        private string? recognitionText;

        // New property for the listening state.
        [ObservableProperty]
        private bool isListening;

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
            _listenCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var isAuthorized = await speechToText.RequestPermissions();
            var usersList = await _manageCacheData.GetUsersAsync();

            // Start animating / showing the indicator.
            IsListening = true;

            if (isAuthorized)
            {
                try
                {
                    RecognitionText = string.Empty;
                    RecognitionText = await speechToText.Listen(
                        CultureInfo.GetCultureInfo(Locale?.Language ?? "en-us"),
                        new Progress<string>(partialText =>
                        {
                            RecognitionText += partialText + " ";
                        }),
                        usersList,
                        _listenCts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Operation was canceled: do nothing special.
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show(cancellationToken);
                }
                finally
                {
                    // Stop showing the listening indicator.
                    IsListening = false;
                }
            }
            else
            {
                await Toast.Make("Permission denied").Show(cancellationToken);
                IsListening = false;
            }
        }
    }
}
