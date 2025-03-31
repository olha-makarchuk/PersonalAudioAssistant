using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Platforms;

namespace PersonalAudioAssistant
{
    public interface ISpeechToText : IAsyncDisposable
    {
        Task<bool> RequestPermissions();
        Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, List<IndividualParameters> allParameters, CancellationToken cancellationToken);
    }

    public static class SpeechToText
    {
        static ISpeechToText? defaultImplementation;

        public static Task<bool> RequestPermissions()
        {
            return Default.RequestPermissions();
        }

        public static Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, List<IndividualParameters> allParameters, CancellationToken cancellationToken)
        {
            return Default.Listen(culture, recognitionResult, allParameters, cancellationToken);
        }

        public static ISpeechToText Default =>
            defaultImplementation ??= new SpeechToTextImplementation();

        internal static void SetDefault(ISpeechToText? implementation) =>
            defaultImplementation = implementation;
    }

    [Table("individual_parameters")]
    public class IndividualParameters 
    {
        [Column("start_phrase")]
        public string StartPhrase { get; set; }

        [Column("end_phrase")]
        public string EndPhrase { get; set; }

        [Column("end_time")]
        public int EndTime { get; set; }

        [Column("voice_id")]
        public string VoiceId { get; set; }

        [Column("reference_voice")]
        public string ReferenceVoice { get; set; }

        [ForeignKey("application_user_id")]
        public int ApplicationUserId { get; set; }
    }
}
