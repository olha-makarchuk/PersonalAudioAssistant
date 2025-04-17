using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery;
using PersonalAudioAssistant.Contracts.Voice;
using System.Collections.ObjectModel;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using PersonalAudioAssistant.Model;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UpdateUserViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IApiClient _apiClient;
        private readonly IMediator _mediator;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private readonly ManageCacheData _manageCacheData;
        private Stream _recordedAudioStream;
        private string _selectedAudioFilePath;
        private string UserIdQueryAttribute;

        [ObservableProperty]
        private ObservableCollection<VoiceResponse> voices = new ObservableCollection<VoiceResponse>();

        private List<VoiceResponse> allVoices = new List<VoiceResponse>();

        public VoiceFilterModel Filter { get; }
        public EndOptionsModel EndOptionsModel { get; }
        public SubUserUpdateModel SubUser { get; }
        public CloneVoiceModel CloneVoiceModel { get; }
        public IsNotValidAddUser IsNotValid { get; }

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

        public UpdateUserViewModel(IMediator mediator, IAudioManager audioManager, ManageCacheData manageCacheData, IApiClient apiClient)
        {
            _mediator = mediator;
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
            _manageCacheData = manageCacheData;
            _apiClient = apiClient;
            Filter = new VoiceFilterModel();
            Filter.PropertyChanged += (_, __) => ApplyVoiceFilter();
            EndOptionsModel = new EndOptionsModel();
            CloneVoiceModel = new CloneVoiceModel();
            IsNotValid = new IsNotValidAddUser();
            SubUser = new SubUserUpdateModel();

            SubUser.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SubUser.UserName))
                    IsNotValid.IsUserNameNotValid = string.IsNullOrWhiteSpace(SubUser.UserName);
                if (e.PropertyName == nameof(SubUser.OldPassword))
                    IsNotValid.IsPasswordNotValid = string.IsNullOrWhiteSpace(SubUser.OldPassword);
                if (e.PropertyName == nameof(SubUser.NewPassword))
                    IsNotValid.IsPasswordNotValid = string.IsNullOrWhiteSpace(SubUser.NewPassword);
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
        }

        public async Task LoadVoicesAsync()
        {
            try
            {
                IsBusy = true; 

                await Task.Delay(10);
                var userId = await SecureStorage.GetAsync("user_id");

                var voiceList = await _mediator.Send(new GetAllVoicesByUserIdQuery()
                {
                    UserId = userId
                });

                var users = await _manageCacheData.GetUsersAsync();
                var user = users.FirstOrDefault(u => u.Id.ToString() == UserIdQueryAttribute);

                SubUser.UserName = user?.UserName;
                SubUser.StartPhrase = user.StartPhrase;
                SubUser.EndPhrase = user.EndPhrase;
                SubUser.EndTime = user.EndTime;
                SubUser.VoiceId = user.VoiceId;
                SubUser.PasswordHash = user.PasswordHash;
                SubUser.Id = user.Id;

                SubUser.IsPasswordEnabled = SubUser.PasswordHash != null;

                if (voiceList != null)
                {
                    allVoices = voiceList;
                    Voices = new ObservableCollection<VoiceResponse>(allVoices);
                    InitializeFilterOptions();

                    if (!string.IsNullOrWhiteSpace(SubUser?.VoiceId))
                    {
                        var userVoice = Voices.FirstOrDefault(v => v.VoiceId == SubUser.VoiceId);
                        if (userVoice != null)
                        {
                            SelectedVoice = userVoice;
                            SelectedVoiceUrl = userVoice.URL;
                        }
                        else
                        {
                            SelectedVoice = Voices.FirstOrDefault();
                            SelectedVoiceUrl = SelectedVoice?.URL;
                        }
                    }
                    else
                    {
                        SelectedVoice = Voices.FirstOrDefault();
                        SelectedVoiceUrl = SelectedVoice?.URL;
                    }
                }
                InitializeFilterOptions();
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
        public async Task UpdateUser()
        {
            if (IsBusy)
                return; 

            try
            {
                IsBusy = true; 

                if (string.IsNullOrWhiteSpace(SubUser.UserName))
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

                if (SelectedVoice == null)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, оберіть голос.", "OK");
                    return;
                }

                var embedding = await _apiClient.CreateVoiceEmbedding(_recordedAudioStream);

                var command = new UpdateSubUserCoomand
                {
                    UserId = await SecureStorage.GetAsync("user_id"),
                    UserName = SubUser.UserName,
                    StartPhrase = SubUser.StartPhrase,
                    EndPhrase = EndOptionsModel.IsEndPhraseSelected ? SubUser.EndPhrase : string.Empty,
                    EndTime = EndOptionsModel.IsEndTimeSelected ? EndOptionsModel.SelectedEndTime.ToString() : string.Empty,
                    VoiceId = SelectedVoice.VoiceId,
                    NewPassword = SubUser.IsPasswordEnabled ? SubUser.NewPassword : string.Empty,
                    Password = SubUser.IsPasswordEnabled ? SubUser.OldPassword : string.Empty,
                    UserVoice = embedding
                };

                if (embedding != null)
                {
                    command.UserVoice = embedding;
                }

                await _mediator.Send(command);
                await _manageCacheData.UpdateUsersList();

                var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService(typeof(UsersListViewModel)) as UsersListViewModel;
                await usersListViewModel?.RefreshUsersAsync();

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

        [RelayCommand]
        public async Task DeleteUser()
        {
            if (IsBusy)
                return;

            var result = await Shell.Current.DisplayAlert("Підтвердження", "Ви впевнені, що хочете видалити цього користувача?", "Так", "Ні");
            if (result)
            {
                try
                {
                    IsBusy = true;
                    var command = new DeleteSubUserCoomand
                    {
                        UserId = SubUser.Id.ToString()
                    };
                    await _mediator.Send(command);
                    await _manageCacheData.UpdateUsersList();

                    var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService(typeof(UsersListViewModel)) as UsersListViewModel;
                    await usersListViewModel.RefreshUsersAsync();
                    await Shell.Current.GoToAsync("//UsersListPage");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Помилка", $"Помилка при видаленні користувача: {ex.Message}", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [RelayCommand]
        public async Task PlaySelectedVoice()
        {
            try
            {
                var mediaElement = ((UpdateUserPage)Shell.Current.CurrentPage).MediaElement;

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

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("userId"))
            {
                UserIdQueryAttribute = query["userId"]?.ToString();
            }
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

        public string SelectedAudioFilePath
        {
            get => _selectedAudioFilePath;
            set => SetProperty(ref _selectedAudioFilePath, value);
        }

        public void OnNavigatedFrom()
        {
            SubUser.UserName = null;
            SelectedVoice = null;
            SelectedVoiceUrl = null;
            EndOptionsModel.IsEndPhraseSelected = false;
            EndOptionsModel.IsEndTimeSelected = true;
            EndOptionsModel.SelectedEndTime = 2;
            SubUser.IsPasswordEnabled = false;
            SubUser.OldPassword = null;
            SubUser.NewPassword = null;
            _recordedAudioStream = null;
            isAudioRecorded = false;
            _selectedAudioFilePath = null;
            IsBaseVoiceSelected = true;
            CloneVoiceModel.IsCloneVoiceSelected = false;

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