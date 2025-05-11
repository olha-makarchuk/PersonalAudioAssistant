using System.Globalization;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.Platforms;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant
{
    public interface ISpeechToText : IAsyncDisposable
    {
        Task<bool> RequestPermissions();
        Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, IProgress<ChatMessage> chatMessageProgress, List<SubUserResponse> listUsers, CancellationToken cancellationToken, Action clearChatMessagesAction, Func<Task> restoreChatMessagesAction, string prevResponseId);
        Task<string> ContinueListen(IProgress<string>? recognitionResult, IProgress<ChatMessage> chatMessageProgress, CancellationToken cancellationToken, Action clearChatMessagesAction, Func<Task> restoreChatMessagesAction, string prevResponseId, ContinueConversation continueConversation);
        void CancelListening();
    }

    public static class SpeechToText
    {
        static ISpeechToText? defaultImplementation;

        public static Task<bool> RequestPermissions()
        {
            return Default.RequestPermissions();
        }

        public static Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, IProgress<ChatMessage> chatMessageProgress, List<SubUserResponse> listUsers, CancellationToken cancellationToken, Action clearChatMessagesAction, Func<Task> restoreChatMessagesAction, string prevResponseId)
        {
            return Default.Listen(culture, recognitionResult, chatMessageProgress, listUsers, cancellationToken, clearChatMessagesAction, restoreChatMessagesAction, prevResponseId);
        }

        public static Task<string> ContinueListen(
            IProgress<string>? recognitionResult,
            IProgress<ChatMessage> chatMessageProgress,
            CancellationToken cancellationToken,
            Action clearChatMessagesAction,
            Func<Task> restoreChatMessagesAction,
            string prevResponseId,
            ContinueConversation continueConversation)
            => Default.ContinueListen(
                recognitionResult,
                chatMessageProgress,
                cancellationToken,
                clearChatMessagesAction,
                restoreChatMessagesAction,
                prevResponseId,
                continueConversation
            );


        public static ISpeechToText Default =>
            defaultImplementation ??= new SpeechToTextImplementation();

        internal static void SetDefault(ISpeechToText? implementation) =>
            defaultImplementation = implementation;
    }
}
