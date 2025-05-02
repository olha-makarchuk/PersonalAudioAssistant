﻿namespace PersonalAudioAssistant.Contracts.Message
{
    public class MessageResponse
    {
        public string messageId { get; set; }
        public string conversationId { get; set; }
        public string text { get; set; }
        public string userRole { get; set; }
        public string audioPath { get; set; }
        public string lastRequestId { get; set; }
        public string subUserId { get; set; }
        public DateTime dateTimeCreated { get; set; }
    }
}
