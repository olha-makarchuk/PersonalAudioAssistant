using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Contracts.Voice;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class AddUserViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly IMediator _mediator;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private readonly ManageCacheData _manageCacheData;
        private Stream _recordedAudioStream;
        private List<VoiceResponse> allVoices = new List<VoiceResponse>();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string filterDescription;

        [ObservableProperty]
        private string filterAge;

        [ObservableProperty]
        private string filterGender;

        [ObservableProperty]
        private string filterUseCase;

        [ObservableProperty]
        private bool isAudioRecorded = false;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private SubUserResponse subUser = new SubUserResponse();

        [ObservableProperty]
        private VoiceResponse selectedVoice;

        [ObservableProperty]
        private string selectedVoiceUrl;

        [ObservableProperty]
        private bool isEndPhraseSelected = false;

        [ObservableProperty]
        private bool isEndTimeSelected = true;

        [ObservableProperty]
        private int selectedEndTime = 2;

        [ObservableProperty]
        private ObservableCollection<VoiceResponse> voices = new ObservableCollection<VoiceResponse>();

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

        [ObservableProperty]
        private bool isPasswordEnabled = false;

        public AddUserViewModel(IMediator mediator, IAudioManager audioManager, ManageCacheData manageCacheData, IApiClient apiClient)
        {
            _mediator = mediator;
            _audioManager = audioManager;
            _audioRecorder = _audioManager.CreateRecorder();
            _manageCacheData = manageCacheData;
            _apiClient = apiClient;

            LoadVoicesAsync();
        }

        [RelayCommand]
        public async Task CreateUserAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

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

                if (_recordedAudioStream == null)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, запишіть голос.", "OK");
                    return;
                }

                if (IsPasswordEnabled && string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, введіть пароль.", "OK");
                    return;
                }

                var embedding = await _apiClient.CreateVoiceEmbedding(_recordedAudioStream);

                var command = new AddSubUserCoomand
                {
                    UserId = await SecureStorage.GetAsync("user_id"),
                    UserName = UserName,
                    Password = IsPasswordEnabled ? Password : null,
                    StartPhrase = SubUser.StartPhrase,
                    EndPhrase = IsEndPhraseSelected ? SubUser.EndPhrase : null,
                    EndTime = IsEndTimeSelected ? SelectedEndTime.ToString() : null,
                    VoiceId = SelectedVoice.VoiceId,
                    UserVoice = embedding
                };
                await _mediator.Send(command);

                await _manageCacheData.UpdateUsersList();

                var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService(typeof(UsersListViewModel)) as UsersListViewModel;
                if (usersListViewModel != null)
                {
                    usersListViewModel.RefreshUsers();
                }

                await Shell.Current.GoToAsync("//UsersListPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadVoicesAsync()
        {
            try
            {
                IsBusy = true;

                var userId = await SecureStorage.GetAsync("user_id");
                var voiceList = await _mediator.Send(new GetAllVoicesByUserIdQuery() 
                { 
                    UserId = userId
                });

                if (voiceList != null)
                {
                    allVoices = voiceList;
                    InitializeFilterOptions(); 
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося завантажити голоси: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task RecordReferenceVoiceAsync()
        {
            try
            {
                IsAudioRecorded = false;

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

                IsAudioRecorded = true;
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

        private void InitializeFilterOptions()
        {
            DescriptionOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Description))
                    .Select(v => v.Description)
                    .Distinct()
                    .OrderBy(x => x)
            );

            AgeOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Age))
                    .Select(v => v.Age)
                    .Distinct()
                    .OrderBy(x => x)
            );

            GenderOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Gender))
                    .Select(v => v.Gender)
                    .Distinct()
                    .OrderBy(x => x)
            );

            UseCaseOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.UseCase))
                    .Select(v => v.UseCase)
                    .Distinct()
                    .OrderBy(x => x)
            );

            ApplyVoiceFilter(); 
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.URL;
        }

        public void OnNavigatedFrom()
        {
            UserName = null;
            SubUser = new SubUserResponse();
            SelectedVoice = null;
            SelectedVoiceUrl = null;
            IsEndPhraseSelected = false;
            IsEndTimeSelected = true;
            SelectedEndTime = 2;
            IsPasswordEnabled = false;
            IsAudioRecorded = false;
            _recordedAudioStream = null;

            ResetDescriptionFilter();
            ResetAgeFilter();
            ResetGenderFilter();
            ResetUseCaseFilter();

            Voices = new ObservableCollection<VoiceResponse>(allVoices);
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.URL;
        }

        [RelayCommand]
        public void ResetAllFilters()
        {
            ResetDescriptionFilter();
            ResetAgeFilter();
            ResetGenderFilter();
            ResetUseCaseFilter();
            ApplyVoiceFilter();
        }

        public ObservableCollection<VoiceResponse> ApplyFilter(List<VoiceResponse> allVoices)
        {
            var filtered = allVoices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterDescription))
                filtered = filtered.Where(v => v.Description != null &&
                    v.Description.Contains(FilterDescription, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(FilterAge))
                filtered = filtered.Where(v => v.Age != null &&
                    v.Age.Contains(FilterAge, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(FilterGender))
                filtered = filtered.Where(v => v.Gender != null &&
                    v.Gender.Contains(FilterGender, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(FilterUseCase))
                filtered = filtered.Where(v => v.UseCase != null &&
                    v.UseCase.Contains(FilterUseCase, StringComparison.OrdinalIgnoreCase));

            return new ObservableCollection<VoiceResponse>(filtered);
        }

        partial void OnSelectedVoiceChanged(VoiceResponse value)
        {
            if (value != null)
            {
                SelectedVoiceUrl = value.URL;
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

        private void ApplyVoiceFilter()
        {
            Voices = ApplyFilter(allVoices);
            SelectedVoice = Voices.FirstOrDefault();
        }

        [RelayCommand]
        public void ResetDescriptionFilter()
        {
            FilterDescription = null;
            ApplyVoiceFilter();
        }

        [RelayCommand]
        public void ResetAgeFilter()
        {
            FilterAge = null;
            ApplyVoiceFilter();
        }

        [RelayCommand]
        public void ResetGenderFilter()
        {
            FilterGender = null;
            ApplyVoiceFilter();
        }

        [RelayCommand]
        public void ResetUseCaseFilter()
        {
            FilterUseCase = null;
            ApplyVoiceFilter();
        }

        partial void OnFilterDescriptionChanged(string value)
        {
            ApplyVoiceFilter();
        }

        partial void OnFilterAgeChanged(string value)
        {
            ApplyVoiceFilter();
        }

        partial void OnFilterGenderChanged(string value)
        {
            ApplyVoiceFilter();
        }

        partial void OnFilterUseCaseChanged(string value)
        {
            ApplyVoiceFilter();
        }
    }
}
