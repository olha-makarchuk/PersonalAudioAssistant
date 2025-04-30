﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Model
{
    public partial class SubUserUpdateModel : ObservableObject
    {
        [ObservableProperty]
        public string? id;

        [ObservableProperty]
        public string? userId;

        [ObservableProperty]
        public string startPhrase;

        [ObservableProperty]
        public string endPhrase;

        [ObservableProperty]
        public string endTime;

        [ObservableProperty]
        public string voiceId;

        [ObservableProperty]
        public List<double> userVoice;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string oldPassword;

        [ObservableProperty]
        private string newPassword;

        [ObservableProperty]
        private bool isPasswordEnabled = false;

        [ObservableProperty]
        public byte[] passwordHash;

        [ObservableProperty]
        public string photoPath;

        public void Reset()
        {
            Id = null;
            UserId = null;
            StartPhrase = string.Empty;
            EndPhrase = string.Empty;
            EndTime = string.Empty;
            VoiceId = string.Empty;
            UserVoice = new List<double>();
            UserName = string.Empty;
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            IsPasswordEnabled = false;
            PasswordHash = null;
            PhotoPath = string.Empty;
        }
    }
}