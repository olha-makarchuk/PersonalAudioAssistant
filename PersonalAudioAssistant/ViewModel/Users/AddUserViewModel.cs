using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Contracts.Voice;
using System.Collections.ObjectModel;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands;

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
        private string _selectedAudioFilePath;

        [ObservableProperty]
        private ObservableCollection<VoiceResponse> voices = new ObservableCollection<VoiceResponse>();

        private List<VoiceResponse> allVoices = new List<VoiceResponse>();
        public VoiceFilterModel Filter { get; }
        public EndOptionsModel EndOptionsModel { get; }
        public SubUserModel SubUser {  get; }
        public CloneVoiceModel CloneVoiceModel { get; }
        public IsNotValidAddUser IsNotValid { get;  }


        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isBaseVoiceSelected = true;

        [ObservableProperty]
        private bool isAudioRecorded = false;

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
            CloneVoiceModel = new CloneVoiceModel();
            IsNotValid = new IsNotValidAddUser();
            SubUser = new SubUserModel();

            SubUser.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SubUser.UserName))
                    IsNotValid.IsUserNameNotValid = string.IsNullOrWhiteSpace(SubUser.UserName);
                if (e.PropertyName == nameof(SubUser.Password))
                    IsNotValid.IsPasswordNotValid = string.IsNullOrWhiteSpace(SubUser.Password);
            };

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsAudioRecorded) && IsAudioRecorded)
                    IsNotValid.IsUserVoiceNotValid = false;
            };
            
            SubUser.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SubUser.StartPhrase))
                    IsNotValid.IsStartPhraseNotValid = string.IsNullOrWhiteSpace(SubUser.StartPhrase);
                if (e.PropertyName == nameof(SubUser.EndPhrase) && EndOptionsModel.IsEndPhraseSelected)
                    IsNotValid.IsEndPhraseNotValid = string.IsNullOrWhiteSpace(SubUser.EndPhrase);
            };

            CloneVoiceModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CloneVoiceModel.IsCloneAudioRecorded) && CloneVoiceModel.IsCloneAudioRecorded)
                    IsNotValid.IsCloneVoiceNotValid = false;
            };

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

                IsNotValid.IsUserNameNotValid = false;
                IsNotValid.IsStartPhraseNotValid = false;
                IsNotValid.IsEndPhraseNotValid = false;
                IsNotValid.IsCloneVoiceNotValid = false;
                IsNotValid.IsUserVoiceNotValid = false;
                IsNotValid.IsPasswordNotValid = false;

                if (string.IsNullOrWhiteSpace(SubUser.UserName))
                    IsNotValid.IsUserNameNotValid = true;

                if (string.IsNullOrWhiteSpace(SubUser.StartPhrase))
                    IsNotValid.IsStartPhraseNotValid = true;

                if (EndOptionsModel.IsEndPhraseSelected && string.IsNullOrWhiteSpace(SubUser.EndPhrase))
                    IsNotValid.IsEndPhraseNotValid = true;

                if (CloneVoiceModel.IsCloneVoiceSelected && _selectedAudioFilePath == null)
                    IsNotValid.IsCloneVoiceNotValid = true;

                if (_recordedAudioStream == null)
                    IsNotValid.IsUserVoiceNotValid = true;

                if (SubUser.IsPasswordEnabled && string.IsNullOrWhiteSpace(SubUser.Password))
                    IsNotValid.IsPasswordNotValid = true;

                if (IsNotValid.IsUserNameNotValid
                    || IsNotValid.IsStartPhraseNotValid
                    || IsNotValid.IsEndPhraseNotValid
                    || IsNotValid.IsCloneVoiceNotValid
                    || IsNotValid.IsUserVoiceNotValid
                    || IsNotValid.IsPasswordNotValid)
                {
                    return;
                }

                var embedding = await _apiClient.CreateVoiceEmbedding(_recordedAudioStream);

                string voiceId;
                if (CloneVoiceModel.IsCloneVoiceSelected)
                {
                    var voiceCloneModel = new ElevenlabsApi();
                    var cloneVoice = await voiceCloneModel.CloneVoiceAsync("назва", _selectedAudioFilePath!);
                    voiceId = cloneVoice.VoiceId;
                    if (CloneVoiceModel.IsCloneVoiceSelected)
                    {
                        var commandVoice = new CreateVoiceCommand()
                        {
                            Name = "name",
                            VoiceId = voiceId
                        };
                        var userId = await _mediator.Send(commandVoice);
                    }
                }
                else
                {
                    voiceId = SelectedVoice.VoiceId;
                }

                var command = new AddSubUserCoomand
                {
                    UserId = await SecureStorage.GetAsync("user_id"),
                    UserName = SubUser.UserName,
                    Password = SubUser.IsPasswordEnabled ? SubUser.Password : null,
                    StartPhrase = SubUser.StartPhrase!,
                    EndPhrase = EndOptionsModel.IsEndPhraseSelected ? SubUser.EndPhrase : null,
                    EndTime = EndOptionsModel.IsEndTimeSelected ? EndOptionsModel.SelectedEndTime.ToString() : null,
                    VoiceId = voiceId,
                    UserVoice = embedding
                };
                var userId = await _mediator.Send(command);

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
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка", "OK");
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
                IsNotValid.IsUserVoiceNotValid = false;
                IsAudioRecorded = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task RecordCloneVoiceAsync()
        {
            try
            {
                CloneVoiceModel.IsCloneAudioRecorded = false;

                if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Мікрофон не доступний", "OK");
                    return;
                }

                await _audioRecorder.StartAsync();
                await Task.Delay(30000);
                var recordedAudio = await _audioRecorder.StopAsync();

                if (recordedAudio == null)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Запис для клонування не вдалося завершити", "OK");
                    return;
                }

                _recordedAudioStream = recordedAudio.GetAudioStream();
                IsNotValid.IsCloneVoiceNotValid = false;
                CloneVoiceModel.IsCloneAudioRecorded = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка при записі для клонування: {ex.Message}", "OK");
            }
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
                    _selectedAudioFilePath = fileResult.FullPath;
                    IsNotValid.IsCloneVoiceNotValid = false;

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
                        var ts = TimeSpan.FromSeconds(Math.Floor(durationSec));
                        CloneVoiceModel.TotalDuration = ts;

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

        public void ResetAllFilters()
        {
            ResetDescriptionFilter();
            ResetAgeFilter();
            ResetGenderFilter();
            ResetUseCaseFilter();
            ApplyVoiceFilter();
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
        public void OnNavigatedFrom()
        {
            SubUser.UserName = null;
            SelectedVoice = null;
            SelectedVoiceUrl = null;
            EndOptionsModel.IsEndPhraseSelected = false;
            EndOptionsModel.IsEndTimeSelected = true;
            EndOptionsModel.SelectedEndTime = 2;
            SubUser.IsPasswordEnabled = false;
            IsAudioRecorded = false;
            _recordedAudioStream = null;
            _selectedAudioFilePath = null;
            SelectedAudioFilePath = null;
            IsBaseVoiceSelected = true;
            CloneVoiceModel.IsRecordSelected = false;
            CloneVoiceModel.IsCloneAudioRecorded = false;
            CloneVoiceModel.IsCloneVoiceSelected = false;
            CloneVoiceModel.IsFragmentSelectionVisible = false;

            IsNotValid.IsUserNameNotValid = false;
            IsNotValid.IsStartPhraseNotValid = false;
            IsNotValid.IsEndPhraseNotValid = false;
            IsNotValid.IsCloneVoiceNotValid = false;
            IsNotValid.IsUserVoiceNotValid = false;
            IsNotValid.IsPasswordNotValid = false;

            ResetDescriptionFilter();
            ResetAgeFilter();
            ResetGenderFilter();
            ResetUseCaseFilter();

            Voices = new ObservableCollection<VoiceResponse>(allVoices);
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.URL;
        }
    }
}
