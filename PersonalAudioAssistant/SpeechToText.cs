using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Platforms;
using Plugin.Maui.Audio;

namespace PersonalAudioAssistant
{
    public interface ISpeechToText : IAsyncDisposable
    {
        Task<bool> RequestPermissions();
        Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, List<SubUser> listUsers, CancellationToken cancellationToken);
    }

    public static class SpeechToText
    {
        static ISpeechToText? defaultImplementation;

        public static Task<bool> RequestPermissions()
        {
            return Default.RequestPermissions();
        }

        public static Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, List<SubUser> listUsers, CancellationToken cancellationToken)
        {
            return Default.Listen(culture, recognitionResult, listUsers, cancellationToken);
        }

        public static ISpeechToText Default =>
            defaultImplementation ??= new SpeechToTextImplementation();

        internal static void SetDefault(ISpeechToText? implementation) =>
            defaultImplementation = implementation;
    }
}
