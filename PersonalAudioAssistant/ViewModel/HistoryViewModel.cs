using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        public HistoryViewModel(IMediator mediator)
        {
            _mediator = mediator;

            LoadUserAsync();
        }

        public async Task LoadUserAsync()
        {

        }
    }
}
