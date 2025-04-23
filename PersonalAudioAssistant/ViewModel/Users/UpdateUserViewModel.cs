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
using System.ComponentModel;

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
        private string _photoUrl;

        [ObservableProperty]
        private ObservableCollection<VoiceResponse> voices = new ObservableCollection<VoiceResponse>();

        [ObservableProperty]
        private VoiceResponse cloneVoice = new();

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

        [ObservableProperty]
        private VoiceResponse selectedCloneVoice;

        [ObservableProperty]
        private bool isVoiceColone;

        [ObservableProperty]
        private bool isVoiceBase;

        [ObservableProperty]
        private string voiceName;

        [ObservableProperty]
        public bool isPhotoSelected;

        [ObservableProperty]
        public bool hasPassword;

        public UpdateUserViewModel(IMediator mediator, IAudioManager audioManager, ManageCacheData manageCacheData, IApiClient apiClient)
        {
            _mediator = mediator;
            _audioManager = audioManager;
            _manageCacheData = manageCacheData;
            _apiClient = apiClient;
            _audioRecorder = audioManager.CreateRecorder();

            Filter = new VoiceFilterModel();
            EndOptionsModel = new EndOptionsModel();
            CloneVoiceModel = new CloneVoiceModel();
            IsNotValid = new IsNotValidAddUser();
            SubUser = new SubUserUpdateModel();

            Filter.PropertyChanged += (_, __) => ApplyVoiceFilter();
            SubUser.PropertyChanged += ValidateSubUser!;
            CloneVoiceModel.PropertyChanged += ValidateCloneModel;

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsAudioRecorded) && IsAudioRecorded)
                    IsNotValid.IsUserVoiceNotValid = false;
                if (e.PropertyName == nameof(IsNotValid.IsPhotoPathNotValid))
                    IsNotValid.IsPhotoPathNotValid = false;
            };
        }

        private void ValidateSubUser(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SubUser.UserName))
                IsNotValid.IsUserNameNotValid = string.IsNullOrWhiteSpace(SubUser.UserName);
            if (e.PropertyName == nameof(SubUser.StartPhrase))
                IsNotValid.IsStartPhraseNotValid = string.IsNullOrWhiteSpace(SubUser.StartPhrase);
            if (e.PropertyName == nameof(SubUser.EndPhrase) && EndOptionsModel.IsEndPhraseSelected)
                IsNotValid.IsEndPhraseNotValid = string.IsNullOrWhiteSpace(SubUser.EndPhrase);
            if (e.PropertyName == nameof(SubUser.NewPassword))
                IsNotValid.IsPasswordNotValid = SubUser.IsPasswordEnabled && string.IsNullOrWhiteSpace(SubUser.NewPassword);

            if (HasPassword == true)
            {
                if (e.PropertyName == nameof(SubUser.OldPassword))
                    IsNotValid.IsPasswordNotValid = SubUser.IsPasswordEnabled && string.IsNullOrWhiteSpace(SubUser.OldPassword);
            }
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

        public async Task LoadVoicesAsync()
        {
            IsBusy = true;

            try
            {
                var appUserId = await SecureStorage.GetAsync("user_id");
                var users = await _manageCacheData.GetUsersAsync();
                var user = users.FirstOrDefault(u => u.Id.ToString() == UserIdQueryAttribute);
                user.PhotoPath = user.PhotoPath.Split('?')[0];
                
                var voiceList = await _mediator.Send(new GetAllVoicesByUserIdQuery()
                {
                    UserId = user.Id.ToString()
                });

                SubUser.UserName = user?.UserName;
                SubUser.StartPhrase = user.StartPhrase;
                SubUser.EndPhrase = user.EndPhrase;
                SubUser.EndTime = user.EndTime;
                SubUser.VoiceId = user.VoiceId;
                SubUser.PasswordHash = user.PasswordHash;
                SubUser.PhotoPath = user.PhotoPath;
                SubUser.Id = user.Id;
                SubUser.UserId = user.UserId;
                SubUser.IsPasswordEnabled = SubUser.PasswordHash != null;
                _photoUrl = user.PhotoPath;
                HasPassword = SubUser.PasswordHash != null;

                if (voiceList != null)
                {
                    allVoices = voiceList;
                    Voices = new ObservableCollection<VoiceResponse>(allVoices);

                    CloneVoice = voiceList.FirstOrDefault(u => u.UserId == user.Id);

                    var userVoiceCommand = new GetVoiceByIdQuery()
                    {
                        VoiceId = user.VoiceId
                    };
                    var userVoice = await _mediator.Send(userVoiceCommand);

                    if (userVoice.UserId == user.Id)
                    {
                        IsVoiceColone = true;
                        IsVoiceBase = false;
                    }
                    else
                    {
                        IsVoiceColone = false;
                        IsVoiceBase = true;
                    }
                    VoiceName = userVoice.Name;

                    InitializeFilterOptions();

                    if (!string.IsNullOrWhiteSpace(SubUser?.VoiceId))
                    {
                        var userVoiceselected = Voices.FirstOrDefault(v => v.VoiceId == SubUser.VoiceId);
                        if (userVoiceselected != null)
                        {
                            SelectedVoice = userVoiceselected;
                            SelectedVoiceUrl = userVoiceselected.URL;
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
        public async Task UpdatePersonalInformation()
        {
            if (IsBusy)
                return;

            try
            {
                if (IsNotValid.IsUserNameNotValid
                   || IsNotValid.IsStartPhraseNotValid
                   || IsNotValid.IsEndPhraseNotValid)
                {
                    return;
                }

                var command = new UpdateSubUserCoomand()
                {
                    Id = SubUser.Id,
                    UserId = SubUser.UserId,
                    UserName = SubUser.UserName,
                    StartPhrase = SubUser.StartPhrase!,
                    EndPhrase = EndOptionsModel.IsEndPhraseSelected ? SubUser.EndPhrase : null,
                    EndTime = EndOptionsModel.IsEndTimeSelected ? EndOptionsModel.SelectedEndTime.ToString() : null
                };

                await _mediator.Send(command);
                await _manageCacheData.UpdateUsersList();
                await Shell.Current.DisplayAlert("Успішно", $"Інформацію оновлено", "OK");

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
        public async Task UpdatePhoto()
        {
            if (IsBusy)
                return;

            try
            {
                if (!IsPhotoSelected)
                    await Shell.Current.DisplayAlert("Помилка", $"Виберіть нову основну світлину", "OK");

                var command = new UpdatePhotoCommand()
                {
                    PhotoPath = SubUser.PhotoPath,
                    PhotoURL = _photoUrl
                };

                await _mediator.Send(command);
                await _manageCacheData.UpdateUsersList();
                await Shell.Current.DisplayAlert("Успішно", $"Інформацію оновлено", "OK");
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
        }

        [RelayCommand]
        public async Task UpdatePassword()
        {
            if (IsBusy)
                return;

            try
            {
                if (HasPassword && !SubUser.IsPasswordEnabled)
                {
                    var currentPwd = await Shell.Current.DisplayPromptAsync(
                        title: "Видалити пароль",
                        message: "Введіть поточний пароль для видалення:",
                        accept: "Видалити",
                        cancel: "Скасувати",
                        placeholder: "Пароль",
                        keyboard: Keyboard.Text);

                    if (string.IsNullOrWhiteSpace(currentPwd))
                        return;
                    
                    var removeCmd = new DeletePasswordSubUserCommand {UserId = SubUser.Id, Password = currentPwd };
                    await _mediator.Send(removeCmd);
                    await Shell.Current.DisplayAlert("Успішно", $"Пароль успішно видалено", "OK");
                    HasPassword = false;
                }

                // User had no password and checked the box -> add new password
                else if (!HasPassword && SubUser.IsPasswordEnabled)
                {
                    if (IsNotValid.IsPasswordNotValid)
                        return;

                    var addCmd = new UpdateSubUserCoomand { NewPassword = SubUser.NewPassword };
                    await _mediator.Send(addCmd);
                    await Shell.Current.DisplayAlert("Успішно", $"Пароль успішно додано", "OK");
                    HasPassword = true;
                }

                // User has password and keeps it enabled -> change password
                else if (HasPassword && SubUser.IsPasswordEnabled)
                {
                    if (IsNotValid.IsPasswordNotValid)
                        return;
                    
                    var changeCmd = new UpdateSubUserCoomand()
                    {
                        Password = SubUser.OldPassword,
                        NewPassword = SubUser.NewPassword
                    };
                    await _mediator.Send(changeCmd);
                    await Shell.Current.DisplayAlert("Успішно", $"Пароль успішно змінено", "OK");
                }
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
        public async Task UpdateUserVoice()
        {
            if (IsBusy)
                return;

            try
            {
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
        public async Task UpdateVoice()
        {
            if (IsBusy)
                return;

            try
            {
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
            IsAudioRecorded = false;
            _selectedAudioFilePath = null;
            IsBaseVoiceSelected = true;
            CloneVoiceModel.IsCloneVoiceSelected = false;

            IsNotValid.IsUserNameNotValid = false;
            IsNotValid.IsStartPhraseNotValid = false;
            IsNotValid.IsEndPhraseNotValid = false;
            IsNotValid.IsCloneVoiceNotValid = false;
            IsNotValid.IsUserVoiceNotValid = false;
            IsNotValid.IsPasswordNotValid = false;
            IsPhotoSelected = false;

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