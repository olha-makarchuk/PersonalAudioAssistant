﻿using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
