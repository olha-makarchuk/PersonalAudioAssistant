using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Domain.Entities;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class AddUserViewModel : ObservableObject
    {
        private readonly IMediator _mediator;

        public AddUserViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Властивості для прив’язки до полів введення
        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string startPhrase;

        // Можна додати додаткові властивості для інших даних користувача
        // [ObservableProperty]
        // private string endPhrase;
        // [ObservableProperty]
        // private string voiceId;

        [RelayCommand]
        private async Task SaveUser()
        {
            // Створення нового користувача із даними з форми
            var newUser = new SubUser
            {
                UserName = UserName,
                StartPhrase = StartPhrase,
                // Присвоєння інших властивостей за потреби
            };

            // Виклик команди через Mediator для збереження користувача (якщо вона реалізована)
            // Наприклад:
            // await _mediator.Send(new CreateUserCommand { SubUser = newUser });

            // Поки що можемо симулювати збереження та перейти до списку користувачів
            await Shell.Current.GoToAsync("//UsersListPage");
        }
    }
}
