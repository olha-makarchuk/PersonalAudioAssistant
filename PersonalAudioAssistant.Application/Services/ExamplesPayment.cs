using System.Text.Json;

namespace PersonalAudioAssistant.Application.Services
{
    public class ExamplesPayment
    {
        public async Task<List<ExamplesPaymentResponse>> GetExamplesPayment()
        {
            string url = "https://audioassistantblob.blob.core.windows.net/audio-message/Examples/ExamplesPayment.json";
            using HttpClient client = new HttpClient();
            List<ExamplesPaymentResponse> examples = new();

            try
            {
                string json = await client.GetStringAsync(url);
                examples = JsonSerializer.Deserialize<List<ExamplesPaymentResponse>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return examples;
        }
    }

    public class ExamplesPaymentResponse
    {
        public string audioRequestPath { get; set; }
        public string audioAnswerPath { get; set; }
        public string textRequest { get; set; }
        public string textAnswer { get; set; }
        public double audioRequestDuration { get; set; }
        public double transcriptionCost { get; set; }
        public double inputCost { get; set; }
        public double outputCost { get; set; }
        public double ttsCost { get; set; }
        public int charCount { get; set; }
        public int inputTokens { get; set; }
        public int outputTokens { get; set; }
    }
}
