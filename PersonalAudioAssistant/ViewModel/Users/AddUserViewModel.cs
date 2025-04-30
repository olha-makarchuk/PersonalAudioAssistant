using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Contracts.Voice;
using System.Collections.ObjectModel;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using MediatR;
using System.ComponentModel;
using PersonalAudioAssistant.Services.Api;
using PersonalAudioAssistant.Services.Api.PersonalAudioAssistant.Services.Api;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class AddUserViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private readonly ManageCacheData _manageCacheData;
        private readonly SubUserApiClient _subUserApiClient;

        private Stream _recordedAudioStream;
        private Stream _recordedCloneAudioStream;
        private string _selectedAudioFilePath;
        private string _cloneVoiceId;

        [ObservableProperty]
        private ObservableCollection<VoiceResponse> voices = new();

        private List<VoiceResponse> allVoices;

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

        [ObservableProperty]
        public string selectedAudioFilePath;

        [ObservableProperty]
        public bool isPhotoSelected;

        VoiceApiClient _voiceApiClient;

        public AddUserViewModel(IAudioManager audioManager, ManageCacheData manageCacheData, IApiClient apiClient, VoiceApiClient voiceApiClient, SubUserApiClient subUserApiClient)
        {
            _audioManager = audioManager;
            _manageCacheData = manageCacheData;
            _apiClient = apiClient;
            _audioRecorder = _audioManager.CreateRecorder();
            _subUserApiClient = subUserApiClient;

            Filter = new VoiceFilterModel();
            EndOptionsModel = new EndOptionsModel();
            CloneVoiceModel = new CloneVoiceModel();
            IsNotValid = new IsNotValidAddUser();
            SubUser = new SubUserModel();
            CloneVoiceModel.IsUploadSelected = true;
            CloneVoiceModel.IsRecordSelected = false;

            Filter.PropertyChanged += (_, __) => ApplyVoiceFilter();
            SubUser.PropertyChanged += ValidateSubUser!;
            CloneVoiceModel.PropertyChanged += ValidateCloneModel;

            _voiceApiClient = voiceApiClient;

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsAudioRecorded) && IsAudioRecorded)
                    IsNotValid.IsUserVoiceNotValid = false;
                if (e.PropertyName == nameof(IsNotValid.IsPhotoPathNotValid))
                    IsNotValid.IsPhotoPathNotValid = false;
            };
        }

        #region Validation
        private void ValidateSubUser(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SubUser.UserName))
                IsNotValid.IsUserNameNotValid = string.IsNullOrWhiteSpace(SubUser.UserName);
            if (e.PropertyName == nameof(SubUser.Password))
                IsNotValid.IsPasswordNotValid = SubUser.IsPasswordEnabled && string.IsNullOrWhiteSpace(SubUser.Password);
            if (e.PropertyName == nameof(SubUser.StartPhrase))
                IsNotValid.IsStartPhraseNotValid = string.IsNullOrWhiteSpace(SubUser.StartPhrase);
            if (e.PropertyName == nameof(SubUser.EndPhrase) && EndOptionsModel.IsEndPhraseSelected)
                IsNotValid.IsEndPhraseNotValid = string.IsNullOrWhiteSpace(SubUser.EndPhrase);
            if (e.PropertyName == nameof(SubUser.PhotoPath))
                IsNotValid.IsPhotoPathNotValid = string.IsNullOrWhiteSpace(SubUser.PhotoPath);
        }

        private void ValidateCloneModel(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CloneVoiceModel.IsCloneAudioRecorded)
                || e.PropertyName == nameof(SelectedAudioFilePath)
                || e.PropertyName == nameof(CloneVoiceModel.Name)
                || e.PropertyName == nameof(CloneVoiceModel.IsUploadSelected)
                || e.PropertyName == nameof(CloneVoiceModel.IsRecordSelected))
            {
                bool isAudioInvalid = false;

                if (CloneVoiceModel.IsUploadSelected)
                {
                    isAudioInvalid = SelectedAudioFilePath == null;
                }
                else if (CloneVoiceModel.IsRecordSelected)
                {
                    isAudioInvalid = !CloneVoiceModel.IsCloneAudioRecorded;
                }

                IsNotValid.IsCloneVoiceNotValid =
                    CloneVoiceModel.IsCloneVoiceSelected &&
                    (string.IsNullOrWhiteSpace(CloneVoiceModel.Name) ||
                     isAudioInvalid);
            }
        }
        #endregion

        #region LoadVoices
        public async Task LoadVoicesAsync()
        {
            IsBusy = true;

            try
            {
                var userId = await SecureStorage.GetAsync("user_id");
                var voiceList = await _voiceApiClient.GetVoicesAsync("");

                if (voiceList != null)
                {
                    allVoices = voiceList.Where(u => u.userId == null).ToList();
                    Voices = new ObservableCollection<VoiceResponse>(allVoices);

                    InitializeFilterOptions();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося завантажити голоси: {ex.Message}", "OK");
            }
            finally{IsBusy = false;}
        }
        #endregion

        #region CreateCloneVoice
        [RelayCommand]
        public async Task CreateCloneVoiceAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                string audioPath;
                IsNotValid.IsCloneVoiceNotValid = false;
                var elevenLabsApi = new ElevenlabsApi();

                if (CloneVoiceModel.IsUploadSelected)
                {
                    if (string.IsNullOrWhiteSpace(_selectedAudioFilePath) || !File.Exists(_selectedAudioFilePath))
                    {
                        IsNotValid.IsCloneVoiceNotValid = true;
                        await Shell.Current.DisplayAlert("Помилка", "Аудіофайл не знайдено або шлях некоректний.", "OK");
                        return;
                    }

                    audioPath = _selectedAudioFilePath;
                }
                else
                {
                    if (_recordedCloneAudioStream == null)
                    {
                        IsNotValid.IsCloneVoiceNotValid = true;
                        await Shell.Current.DisplayAlert("Помилка", "Запис аудіо не знайдено.", "OK");
                        return;
                    }

                    audioPath = Path.Combine(FileSystem.CacheDirectory, $"clone_{Guid.NewGuid()}.wav");

                    using (var fileStream = File.Create(audioPath))
                    {
                        _recordedCloneAudioStream.Position = 0;
                        await _recordedCloneAudioStream.CopyToAsync(fileStream);
                    }
                }

                if (CloneVoiceModel.IsCloneGenerated)
                {
                    await elevenLabsApi.DeleteVoiceAsync(_cloneVoiceId);
                    CloneVoiceModel.IsCloneGenerated = false;
                }

                var clonedVoiceId = await elevenLabsApi.CloneVoiceAsync(
                    CloneVoiceModel.Name,
                    audioPath
                );

                if (!string.IsNullOrEmpty(clonedVoiceId.VoiceId))
                {
                    _cloneVoiceId = clonedVoiceId.VoiceId;
                    CloneVoiceModel.IsCloneGenerated = true;
                    IsNotValid.IsCloneVoiceNotValid = false;
                    await Shell.Current.DisplayAlert("Успіх", "Голос успішно клоновано!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Помилка", "Не вдалося клонувати голос. Спробуйте пізніше.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Під час клонування сталася помилка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion

        #region CreateUser
        [RelayCommand]
        public async Task CreateUserAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                IsNotValid.IsUserNameNotValid = false;
                IsNotValid.IsStartPhraseNotValid = false;
                IsNotValid.IsEndPhraseNotValid = false;
                IsNotValid.IsCloneVoiceNotValid = false;
                IsNotValid.IsUserVoiceNotValid = false;
                IsNotValid.IsPasswordNotValid = false;
                IsNotValid.IsPhotoPathNotValid = false;

                if (string.IsNullOrWhiteSpace(SubUser.UserName))
                    IsNotValid.IsUserNameNotValid = true;

                if (string.IsNullOrWhiteSpace(SubUser.StartPhrase))
                    IsNotValid.IsStartPhraseNotValid = true;

                if (EndOptionsModel.IsEndPhraseSelected && string.IsNullOrWhiteSpace(SubUser.EndPhrase))
                    IsNotValid.IsEndPhraseNotValid = true;

                if (CloneVoiceModel.IsCloneVoiceSelected && CloneVoiceModel.IsCloneGenerated != true)
                    IsNotValid.IsCloneVoiceNotValid = true;

                if (_recordedAudioStream == null)
                    IsNotValid.IsUserVoiceNotValid = true;

                if (SubUser.IsPasswordEnabled && string.IsNullOrWhiteSpace(SubUser.Password))
                    IsNotValid.IsPasswordNotValid = true;

                if (string.IsNullOrWhiteSpace(SubUser.PhotoPath))
                    IsNotValid.IsPhotoPathNotValid = true;

                if (IsNotValid.IsUserNameNotValid
                    || IsNotValid.IsStartPhraseNotValid
                    || IsNotValid.IsEndPhraseNotValid
                    || IsNotValid.IsCloneVoiceNotValid
                    || IsNotValid.IsUserVoiceNotValid
                    || IsNotValid.IsPasswordNotValid
                    || IsNotValid.IsPhotoPathNotValid)
                {
                    return;
                }

                var embedding = await _apiClient.CreateVoiceEmbedding(_recordedAudioStream);
                CreatedVoice createdVoicevoiceId = new();

                if (CloneVoiceModel.IsCloneVoiceSelected)
                {
                    createdVoicevoiceId = await _voiceApiClient.CreateVoiceAsync(_cloneVoiceId, CloneVoiceModel.Name, "");
                }
                else
                {
                    createdVoicevoiceId.voiceId = SelectedVoice.id;
                }

                var command = new AddSubUserCommand()
                {
                    UserId = await SecureStorage.GetAsync("user_id"),
                    UserName = SubUser.UserName,
                    Password = SubUser.IsPasswordEnabled ? SubUser.Password : null,
                    StartPhrase = SubUser.StartPhrase!,
                    EndPhrase = EndOptionsModel.IsEndPhraseSelected ? SubUser.EndPhrase : null,
                    EndTime = EndOptionsModel.IsEndTimeSelected ? EndOptionsModel.SelectedEndTime.ToString() : null,
                    VoiceId = createdVoicevoiceId.voiceId,
                    PhotoPath = SubUser.PhotoPath,
                    UserVoice = embedding
                };
                var userId =  await _subUserApiClient.AddSubUserAsync(command);

                if (CloneVoiceModel.IsCloneVoiceSelected)
                {
                    await _voiceApiClient.UpdateVoiceAsync(createdVoicevoiceId.voiceId, userId);
                }

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
        #endregion

        #region PlayAudio_RecordAudio
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

                _recordedCloneAudioStream = recordedAudio.GetAudioStream();
                IsNotValid.IsCloneVoiceNotValid = false;
                CloneVoiceModel.IsCloneAudioRecorded = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка при записі для клонування: {ex.Message}", "OK");
            }
        }
        #endregion

        #region PickPhoto_AudioFile
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
                        { DevicePlatform.iOS,    new[] { "public.audio" } },
                        { DevicePlatform.WinUI,  new[] { ".mp3", ".wav", ".aac", ".m4a" } },
                        { DevicePlatform.Tizen,  new[] { "*/*" } },
                        { DevicePlatform.macOS,  new[] { "public.audio" } },
                    })
                });

                if (fileResult == null)
                    return; 

                _selectedAudioFilePath = fileResult.FullPath;

                SelectedAudioFilePath = Path.GetFileName(_selectedAudioFilePath);
                OnPropertyChanged(nameof(SelectedAudioFilePath));

                IsNotValid.IsCloneVoiceNotValid = false;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося обрати файл: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            var customImageFileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" } },
                    { DevicePlatform.Android, new[] { "image/*" } },
                    { DevicePlatform.iOS, new[] { "public.image" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.image" } },
                    { DevicePlatform.Tizen, new[] { "*/*" } }
                });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть будь-яке зображення",
                FileTypes = customImageFileTypes
            });

            if (result != null)
            {
                SubUser.PhotoPath = result.FullPath;
                IsPhotoSelected = true;
                OnPropertyChanged(nameof(SubUser));
                OnPropertyChanged(nameof(SubUser.PhotoPath));
            }
            IsNotValid.IsPhotoPathNotValid = false;
        }
        #endregion

        #region Filter
        public ObservableCollection<VoiceResponse> ApplyFilter(List<VoiceResponse> allVoices)
        {
            var filtered = allVoices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(Filter.Description))
                filtered = filtered.Where(v => v.description != null &&
                    v.description.Contains(Filter.Description, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Filter.Age))
                filtered = filtered.Where(v => v.age != null &&
                    v.age.Contains(Filter.Age, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Filter.Gender))
                filtered = filtered.Where(v => v.gender != null &&
                    v.gender.Contains(Filter.Gender, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Filter.UseCase))
                filtered = filtered.Where(v => v.useCase != null &&
                    v.useCase.Contains(Filter.UseCase, StringComparison.OrdinalIgnoreCase));

            return new ObservableCollection<VoiceResponse>(filtered);
        }

        private void InitializeFilterOptions()
        {
            Filter.DescriptionOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.description))
                    .Select(v => v.description)
                    .Distinct()
                    .OrderBy(x => x)
            );

            Filter.AgeOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.age))
                    .Select(v => v.age)
                    .Distinct()
                    .OrderBy(x => x)
            );

            Filter.GenderOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.gender))
                    .Select(v => v.gender)
                    .Distinct()
                    .OrderBy(x => x)
            );

            Filter.UseCaseOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.useCase))
                    .Select(v => v.useCase)
                    .Distinct()
                    .OrderBy(x => x)
            );

            ApplyVoiceFilter();
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.url;
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
                SelectedVoiceUrl = value.url;
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
        #endregion

        [RelayCommand]
        public void OnNavigatedFrom()
        {
            SelectedVoice = null;
            SelectedVoiceUrl = null;
            IsPhotoSelected = false;
            EndOptionsModel.Reset();
            IsAudioRecorded = false;
            _recordedAudioStream = null;
            _selectedAudioFilePath = null;
            SelectedAudioFilePath = null;
            IsBaseVoiceSelected = true;
            CloneVoiceModel.Reset();
            SubUser.Reset();
            CloneVoiceModel.Reset();
            IsNotValid.Reset();

            ResetDescriptionFilter();
            ResetAgeFilter();
            ResetGenderFilter();
            ResetUseCaseFilter();

            Voices = new ObservableCollection<VoiceResponse>(allVoices);
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.url;
        }
    }
}
