using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PersonalAudioAssistant.Services;
using MediatR;

namespace PersonalAudioAssistant
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
