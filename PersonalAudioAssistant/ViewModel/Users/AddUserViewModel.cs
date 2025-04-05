using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Application.Services.Api;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class AddUserViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private Stream _recordedAudioStream;
        private List<Voice> allVoices = new List<Voice>();

        [ObservableProperty]
        private string filterAccent;

        [ObservableProperty]
        private string filterDescription;

        [ObservableProperty]
        private string filterAge;

        [ObservableProperty]
        private string filterGender;

        [ObservableProperty]
        private string filterUseCase;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string password;
        
        [ObservableProperty]
        private SubUser subUser = new SubUser();

        [ObservableProperty]
        private Voice selectedVoice;

        [ObservableProperty]
        private string selectedVoiceUrl;

        [ObservableProperty]
        private bool isEndPhraseSelected = false;

        [ObservableProperty]
        private bool isEndTimeSelected = true;

        [ObservableProperty]
        private int selectedEndTime = 2;

        [ObservableProperty]
        private ObservableCollection<Voice> voices = new ObservableCollection<Voice>();

        [ObservableProperty]
        private ObservableCollection<int> endTimeOptions = new ObservableCollection<int>(Enumerable.Range(2, 9));

        [ObservableProperty]
        private ObservableCollection<string> accentOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> descriptionOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> ageOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> genderOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> useCaseOptions = new ObservableCollection<string>();

        public AddUserViewModel(IMediator mediator, IAudioManager audioManager)
        {
            _mediator = mediator;
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
            LoadVoicesAsync();
        }

        partial void OnSelectedVoiceChanged(Voice value)
        {
            if (value != null)
            {
                SelectedVoiceUrl = value.preview_url;
            }
        }

        partial void OnIsEndPhraseSelectedChanged(bool value)
        {
            if (value)
            {
                IsEndTimeSelected = false;
            }
        }

        partial void OnIsEndTimeSelectedChanged(bool value)
        {
            if (value)
            {
                IsEndPhraseSelected = false;
            }
        }

        [RelayCommand]
        public async Task CreateUserAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                await Shell.Current.DisplayAlert("Помилка", "Ім'я користувача не може бути порожнім.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SubUser.StartPhrase))
            {
                await Shell.Current.DisplayAlert("Помилка", "Початкова фраза не може бути порожньою.", "OK");
                return;
            }

            if (IsEndPhraseSelected && string.IsNullOrWhiteSpace(SubUser.EndPhrase))
            {
                await Shell.Current.DisplayAlert("Помилка", "Кінцева фраза обрана, але не заповнена.", "OK");
                return;
            }

            if (IsEndTimeSelected && SelectedEndTime <= 0)
            {
                await Shell.Current.DisplayAlert("Помилка", "Час завершення має бути більше 0.", "OK");
                return;
            }

            if (SelectedVoice == null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, оберіть голос.", "OK");
                return;
            }

            if(_recordedAudioStream == null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, запишіть голос.", "OK");
                return;
            }

            if (password == null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, введіть пароль.", "OK");
                return;
            }

            var command = new AddSubUserCoomand
            {
                UserName = UserName,
                Password = Password,
                StartPhrase = SubUser.StartPhrase,
                EndPhrase = IsEndPhraseSelected ? SubUser.EndPhrase : string.Empty,
                EndTime = IsEndTimeSelected ? SelectedEndTime.ToString() : string.Empty,
                VoiceId = SelectedVoice.voice_id,
                UserVoice = new byte[0]
            };
            await _mediator.Send(command);

            // Отримання UsersListViewModel та виклик RefreshUsers
            var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService<UsersListViewModel>();
            if (usersListViewModel != null)
            {
                usersListViewModel.RefreshUsers();
            }

            await Shell.Current.GoToAsync("//UsersListPage");
        }

        private async void LoadVoicesAsync()
        {
            try
            {
                VoicesApi apiClient = new VoicesApi();
                var voiceList = await apiClient.GetVoicesAsync();

                if (voiceList != null)
                {
                    allVoices = voiceList;
                    Voices = new ObservableCollection<Voice>(voiceList);
                    SelectedVoice = Voices.FirstOrDefault();
                    SelectedVoiceUrl = SelectedVoice?.preview_url;

                    InitializeFilterOptions();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося завантажити голоси: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task RecordReferenceVoiceAsync()
        {
            try
            {
                if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Мікрофон не доступний", "OK");
                    return;
                }

                await _audioRecorder.StartAsync();
                await Task.Delay(5000); 
                var recordedAudio = await _audioRecorder.StopAsync();

                if (recordedAudio == null)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Запис не вдалося завершити", "OK");
                    return;
                }

                _recordedAudioStream = recordedAudio.GetAudioStream();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task PlaySelectedVoiceAsync()
        {
            try
            {
                var mediaElement = ((AddUserPage)Shell.Current.CurrentPage).MediaElement;

                if (!string.IsNullOrEmpty(SelectedVoiceUrl))
                {
                    mediaElement.Source = SelectedVoiceUrl;
                }
                if (mediaElement.CurrentState == CommunityToolkit.Maui.Core.Primitives.MediaElementState.Playing)
                    mediaElement.Pause();
                else
                    mediaElement.Play();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відтворити голос: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public void ApplyFilter()
        {
            var filtered = allVoices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterAccent))
                filtered = filtered.Where(v => v.labels?.accent?.Contains(FilterAccent, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterDescription))
                filtered = filtered.Where(v => v.labels?.description?.Contains(FilterDescription, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterAge))
                filtered = filtered.Where(v => v.labels?.age?.Contains(FilterAge, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterGender))
                filtered = filtered.Where(v => v.labels?.gender?.Contains(FilterGender, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterUseCase))
                filtered = filtered.Where(v => v.labels?.use_case?.Contains(FilterUseCase, StringComparison.OrdinalIgnoreCase) == true);

            Voices = new ObservableCollection<Voice>(filtered);
            SelectedVoice = Voices.FirstOrDefault();
        }

        public void OnNavigatedFrom()
        {
            UserName = null;
            SubUser = new SubUser();
            SelectedVoice = null;
            SelectedVoiceUrl = null;
            IsEndPhraseSelected = false;
            IsEndTimeSelected = true;
            SelectedEndTime = 2;
            FilterAccent = null;
            FilterDescription = null;
            FilterAge = null;
            FilterGender = null;
            FilterUseCase = null;

            Voices = new ObservableCollection<Voice>(allVoices);
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.preview_url;
        }

        private void InitializeFilterOptions()
        {
            AccentOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.labels?.accent))
                    .Select(v => v.labels.accent)
                    .Distinct()
                    .OrderBy(x => x)
            );

            DescriptionOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.labels?.description))
                    .Select(v => v.labels.description)
                    .Distinct()
                    .OrderBy(x => x)
            );

            AgeOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.labels?.age))
                    .Select(v => v.labels.age)
                    .Distinct()
                    .OrderBy(x => x)
            );

            GenderOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.labels?.gender))
                    .Select(v => v.labels.gender)
                    .Distinct()
                    .OrderBy(x => x)
            );

            UseCaseOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.labels?.use_case))
                    .Select(v => v.labels.use_case)
                    .Distinct()
                    .OrderBy(x => x)
            );
        }

        partial void OnFilterDescriptionChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnFilterAgeChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnFilterGenderChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnFilterUseCaseChanged(string value)
        {
            ApplyFilter();
        }
        partial void OnFilterAccentChanged(string value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        public void ResetAccentFilter()
        {
            FilterAccent = null;
            ApplyFilter();
        }

        [RelayCommand]
        public void ResetDescriptionFilter()
        {
            FilterDescription = null;
            ApplyFilter();
        }

        [RelayCommand]
        public void ResetAgeFilter()
        {
            FilterAge = null;
            ApplyFilter();
        }

        [RelayCommand]
        public void ResetGenderFilter()
        {
            FilterGender = null;
            ApplyFilter();
        }

        [RelayCommand]
        public void ResetUseCaseFilter()
        {
            FilterUseCase = null;
            ApplyFilter();
        }
    }
}
