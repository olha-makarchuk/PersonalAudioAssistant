using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Contracts.Voice;
using System.Collections.ObjectModel;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using MediatR;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class AddUserViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly IMediator _mediator;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private Stream _recordedAudioStream;
        private string _selectedAudioFilePath;
        private string _cloneVoiceId;
        private readonly ManageCacheData _manageCacheData;

        private List<VoiceResponse> allVoices = new List<VoiceResponse>();
        public VoiceFilterModel Filter { get; }
        public EndOptionsModel EndOptionsModel { get; }
        public CredentialsModel CredentialsModel { get; }
        public CloneVoiceModel CloneVoiceModel { get; }

        [ObservableProperty]
        private ObservableCollection<VoiceResponse> voices = new ObservableCollection<VoiceResponse>();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isBaseVoiceSelected = true;

        [ObservableProperty]
        private bool isAudioRecorded = false;

        [ObservableProperty]
        private SubUserResponse subUser = new SubUserResponse();

        [ObservableProperty]
        private VoiceResponse selectedVoice;

        [ObservableProperty]
        private string selectedVoiceUrl;

        public AddUserViewModel(IMediator mediator, IAudioManager audioManager, ManageCacheData manageCacheData, IApiClient apiClient)
        {
            _mediator = mediator;
            _audioManager = audioManager;
            _audioRecorder = _audioManager.CreateRecorder();
            _manageCacheData = manageCacheData;
            _apiClient = apiClient;
            Filter = new VoiceFilterModel();
            Filter.PropertyChanged += (_, __) => ApplyVoiceFilter();
            EndOptionsModel = new EndOptionsModel();
            CredentialsModel = new CredentialsModel();
            CloneVoiceModel = new CloneVoiceModel();

            LoadVoicesAsync();
        }

        [RelayCommand]
        public async Task CreateVoiceCloneAsync()
        {
            if (_selectedAudioFilePath == null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, виберіть файл для клонування.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                var tts = new ElevenlabsApi();
                var result = await tts.CloneVoiceAsync(
                    filePath: _selectedAudioFilePath,
                    voiceName: $"{CredentialsModel.UserName}_Clone"
                );
                _cloneVoiceId = result.VoiceId;
                CloneVoiceModel.IsCloneGenerated = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося клонувати голос: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task CreateUserAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(CredentialsModel.UserName))
                {
                    await Shell.Current.DisplayAlert("Помилка", "Ім'я користувача не може бути порожнім.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SubUser.StartPhrase))
                {
                    await Shell.Current.DisplayAlert("Помилка", "Початкова фраза не може бути порожньою.", "OK");
                    return;
                }

                if (EndOptionsModel.IsEndPhraseSelected && string.IsNullOrWhiteSpace(SubUser.EndPhrase))
                {
                    await Shell.Current.DisplayAlert("Помилка", "Кінцева фраза обрана, але не заповнена.", "OK");
                    return;
                }

                if (EndOptionsModel.IsEndTimeSelected && EndOptionsModel.SelectedEndTime <= 0)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Час завершення має бути більше 0.", "OK");
                    return;
                }

                if (CloneVoiceModel.IsCloneVoiceSelected && !CloneVoiceModel.IsCloneGenerated)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, спочатку створіть клон голосу.", "OK");
                    return;
                }

                if (SelectedVoice == null && !CloneVoiceModel.IsCloneVoiceSelected)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, оберіть голос.", "OK");
                    return;
                }

                if (_recordedAudioStream == null)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, запишіть голос.", "OK");
                    return;
                }

                if (CredentialsModel.IsPasswordEnabled && string.IsNullOrWhiteSpace(CredentialsModel.Password))
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, введіть пароль.", "OK");
                    return;
                }

                var embedding = await _apiClient.CreateVoiceEmbedding(_recordedAudioStream);

                var voiceIdToUse = CloneVoiceModel.IsCloneVoiceSelected ? _cloneVoiceId : SelectedVoice.VoiceId;

                var command = new AddSubUserCoomand
                {
                    UserId = await SecureStorage.GetAsync("user_id"),
                    UserName = CredentialsModel.UserName,
                    Password = CredentialsModel.IsPasswordEnabled ? CredentialsModel.Password : null,
                    StartPhrase = SubUser.StartPhrase,
                    EndPhrase = EndOptionsModel.IsEndPhraseSelected ? SubUser.EndPhrase : null,
                    EndTime = EndOptionsModel.IsEndTimeSelected ? EndOptionsModel.SelectedEndTime.ToString() : null,
                    VoiceId = voiceIdToUse,
                    UserVoice = embedding
                };
                await _mediator.Send(command);

                await _manageCacheData.UpdateUsersList();

                var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService(typeof(UsersListViewModel)) as UsersListViewModel;
                if (usersListViewModel != null)
                {
                    await usersListViewModel.RefreshUsersAsync();
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
                await Task.Delay(10000);
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
            Filter.DescriptionOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Description))
                    .Select(v => v.Description)
                    .Distinct()
                    .OrderBy(x => x)
            );

            Filter.AgeOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Age))
                    .Select(v => v.Age)
                    .Distinct()
                    .OrderBy(x => x)
            );

            Filter.GenderOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Gender))
                    .Select(v => v.Gender)
                    .Distinct()
                    .OrderBy(x => x)
            );

            Filter.UseCaseOptions = new ObservableCollection<string>(
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

        [RelayCommand]
        private async Task PickAudioFileAsync()
        {
            try
            {
                var fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/*" } },
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".aac", ".m4a" } },
                { DevicePlatform.Tizen, new[] { "*/*" } },
                { DevicePlatform.macOS, new[] { "public.audio" } },
            })
                });

                if (fileResult != null)
                {
                    // 1) Зберігаємо шлях і ім'я
                    _selectedAudioFilePath = fileResult.FullPath;
                    SelectedAudioFilePath = fileResult.FileName;
                    OnPropertyChanged(nameof(SelectedAudioFilePath));

                    var cacheFile = Path.Combine(FileSystem.CacheDirectory, fileResult.FileName);
                    using (var inStream = await fileResult.OpenReadAsync())
                    using (var outFs = File.OpenWrite(cacheFile))
                        await inStream.CopyToAsync(outFs);

                    var player = _audioManager.CreatePlayer(cacheFile);
                    var durationSec = player.Duration;

                    CloneVoiceModel.IsFragmentSelectionVisible = durationSec > 30;
                    player.Dispose();

                    if (durationSec > 30)
                    {
                        var ts = TimeSpan.FromSeconds(durationSec);

                        CloneVoiceModel.HourOptions.Clear();
                        for (int h = 0; h <= ts.Hours; h++)
                            CloneVoiceModel.HourOptions.Add(h);

                        CloneVoiceModel.MinuteOptions.Clear();
                        CloneVoiceModel.SecondOptions.Clear();
                        for (int i = 0; i < 60; i++)
                        {
                            CloneVoiceModel.MinuteOptions.Add(i);
                            CloneVoiceModel.SecondOptions.Add(i);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Помилка при виборі файлу: {ex.Message}", "OK");
            }
        }


        public void OnNavigatedFrom()
        {
            CredentialsModel.UserName = null;
            SubUser = new SubUserResponse();
            SelectedVoice = null;
            SelectedVoiceUrl = null;
            EndOptionsModel.IsEndPhraseSelected = false;
            EndOptionsModel.IsEndTimeSelected = true;
            EndOptionsModel.SelectedEndTime = 2;
            CredentialsModel.IsPasswordEnabled = false;
            IsAudioRecorded = false;
            _recordedAudioStream = null;
            _selectedAudioFilePath = null;
            SelectedAudioFilePath = null;
            IsBaseVoiceSelected = true;
            CloneVoiceModel.IsRecordSelected = false;
            CloneVoiceModel.IsCloneAudioRecorded = false;
            CloneVoiceModel.IsCloneVoiceSelected = false;
            CloneVoiceModel.IsCloneGenerated = false;
            CloneVoiceModel.IsFragmentSelectionVisible = false;

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

            if (!string.IsNullOrWhiteSpace(Filter.Description))
                filtered = filtered.Where(v => v.Description != null &&
                    v.Description.Contains(Filter.Description, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Filter.Age))
                filtered = filtered.Where(v => v.Age != null &&
                    v.Age.Contains(Filter.Age, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Filter.Gender))
                filtered = filtered.Where(v => v.Gender != null &&
                    v.Gender.Contains(Filter.Gender, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Filter.UseCase))
                filtered = filtered.Where(v => v.UseCase != null &&
                    v.UseCase.Contains(Filter.UseCase, StringComparison.OrdinalIgnoreCase));

            return new ObservableCollection<VoiceResponse>(filtered);
        }

        partial void OnSelectedVoiceChanged(VoiceResponse value)
        {
            if (value != null)
            {
                SelectedVoiceUrl = value.URL;
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
            Filter.Description = null;
            ApplyVoiceFilter();
        }

        [RelayCommand]
        public void ResetAgeFilter()
        {
            Filter.Age = null;
            ApplyVoiceFilter();
        }

        [RelayCommand]
        public void ResetGenderFilter()
        {
            Filter.Gender = null;
            ApplyVoiceFilter();
        }

        [RelayCommand]
        public void ResetUseCaseFilter()
        {
            Filter.UseCase = null;
            ApplyVoiceFilter();
        }
       
        public string SelectedAudioFilePath
        {
            get => _selectedAudioFilePath;
            set => SetProperty(ref _selectedAudioFilePath, value);
        }

        [RelayCommand]
        public async Task RecordCloneVoiceAsync()
        {
            try
            {
                CloneVoiceModel.IsCloneAudioRecorded = false;
                /*
                if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Мікрофон не доступний", "OK");
                    return;
                }

                await _audioRecorder.StartAsync();

                if (int.TryParse(CloneVoiceModel.CloneStart, out var start) &&
                    int.TryParse(CloneVoiceModel.CloneEnd, out var end) &&
                    start >= 0 && end > start)
                {
                    await Task.Delay((end - start) * 1000);
                }
                else
                {
                    await Task.Delay(10000);
                }

                var recordedAudio = await _audioRecorder.StopAsync();
                if (recordedAudio == null)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Запис для клонування не вдалося завершити", "OK");
                    return;
                }

                _cloneRecordedAudioStream = recordedAudio.GetAudioStream();*/
                CloneVoiceModel.IsCloneAudioRecorded = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка при записі для клонування: {ex.Message}", "OK");
            }
        }
    }
}
