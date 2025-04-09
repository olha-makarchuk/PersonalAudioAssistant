using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Maui.Controls;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using PersonalAudioAssistant.Application.Services.Api;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UpdateUserViewModel : ObservableObject, IQueryAttributable
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
        private string oldPassword;

        [ObservableProperty]
        private string newPassword;

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

        public UpdateUserViewModel(IMediator mediator, IAudioManager audioManager)
        {
            _mediator = mediator;
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
            LoadVoicesAsync();
        }

        // Реакція на зміну вибраного голосу
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
        public async Task UpdateUserCommand()
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

            if (_recordedAudioStream == null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, запишіть голос.", "OK");
                return;
            }

            var command = new UpdateSubUserCoomand
            {
                UserName = UserName,
                StartPhrase = SubUser.StartPhrase,
                EndPhrase = IsEndPhraseSelected ? SubUser.EndPhrase : string.Empty,
                EndTime = IsEndTimeSelected ? SelectedEndTime.ToString() : string.Empty,
                VoiceId = SelectedVoice.voice_id,
                UserVoice = new byte[0],
                NewPassword = NewPassword,
                Password = OldPassword
            };
            await _mediator.Send(command);

            // Оновлення списку користувачів
            var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService(typeof(UsersListViewModel)) as UsersListViewModel;
            usersListViewModel?.RefreshUsers();

            await Shell.Current.GoToAsync("//UsersListPage");
        }

        [RelayCommand]
        public async Task DeleteUser()
        {
            var result = await Shell.Current.DisplayAlert("Підтвердження", "Ви впевнені, що хочете видалити цього користувача?", "Так", "Ні");
            if (result)
            {
                var command = new DeleteSubUserCoomand
                {
                    UserId = SubUser.Id.ToString()
                };
                await _mediator.Send(command);

                var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService(typeof(UsersListViewModel)) as UsersListViewModel;
                usersListViewModel?.RefreshUsers();
                await Shell.Current.GoToAsync("//UsersListPage");
            }
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

                    // Якщо SubUser має вказаний голос (наприклад, через властивість VoiceId), то шукаємо цей голос у списку.
                    if (!string.IsNullOrWhiteSpace(SubUser?.VoiceId))
                    {
                        var userVoice = Voices.FirstOrDefault(v => v.voice_id == SubUser.VoiceId);
                        if (userVoice != null)
                        {
                            SelectedVoice = userVoice;
                            SelectedVoiceUrl = userVoice.preview_url;
                        }
                        else
                        {
                            // Якщо голос за ідентифікатором не знайдений – встановлюємо перший голос.
                            SelectedVoice = Voices.FirstOrDefault();
                            SelectedVoiceUrl = SelectedVoice?.preview_url;
                        }
                    }
                    else
                    {
                        SelectedVoice = Voices.FirstOrDefault();
                        SelectedVoiceUrl = SelectedVoice?.preview_url;
                    }

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
        public void ApplyFilter()
        {
            var filtered = allVoices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterAccent))
                filtered = filtered.Where(v => v.labels?.accent?.Contains(FilterAccent, System.StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterDescription))
                filtered = filtered.Where(v => v.labels?.description?.Contains(FilterDescription, System.StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterAge))
                filtered = filtered.Where(v => v.labels?.age?.Contains(FilterAge, System.StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterGender))
                filtered = filtered.Where(v => v.labels?.gender?.Contains(FilterGender, System.StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(FilterUseCase))
                filtered = filtered.Where(v => v.labels?.use_case?.Contains(FilterUseCase, System.StringComparison.OrdinalIgnoreCase) == true);

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
            OldPassword = null;
            NewPassword = null;

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

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("userId"))
            {
                var userId = query["userId"]?.ToString();

                var userQuery = new GetUserByIdQuery()
                {
                    UserId = userId
                };

                var user = await _mediator.Send(userQuery);

                UserName = user.UserName;
                SubUser = user;
            }
        }
    }
}
