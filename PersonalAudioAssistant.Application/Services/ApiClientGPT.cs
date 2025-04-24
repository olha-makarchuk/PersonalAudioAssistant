using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Application.Services
{
    public class ApiClientGPT
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiKey = "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"; 
        private readonly string endpoint = "https://api.openai.com/v1/responses";

        public async Task<ApiClientGptResponse> ContinueChatAsync(string userMessage, string prevResponseId = null)
        {
            var requestPayload = new
            {
                model = "gpt-4o-mini",
                input = new[] {
                new {
                    role = "user",
                    content = userMessage
                }
            },
                store = true,
                previous_response_id = prevResponseId
            };

            var json = JsonConvert.SerializeObject(requestPayload,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var responseApi = await client.SendAsync(request);
            responseApi.EnsureSuccessStatusCode();

            Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(await responseApi.Content.ReadAsStringAsync());

            var response = new ApiClientGptResponse
            {
                responseId = myDeserializedClass.id,
                text = myDeserializedClass.output[0].content[0].text
            };

            return response;
        }
    }
     
    public class ApiClientGptResponse
    {
        public string responseId { get; set; }
        public string text { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public string text { get; set; }
        public List<object> annotations { get; set; }
    }

    public class Format
    {
        public string type { get; set; }
    }

    public class InputTokensDetails
    {
        public int cached_tokens { get; set; }
    }

    public class Metadata
    {
    }

    public class Output
    {
        public string type { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string role { get; set; }
        public List<Content> content { get; set; }
    }

    public class OutputTokensDetails
    {
        public int reasoning_tokens { get; set; }
    }

    public class Reasoning
    {
        public object effort { get; set; }
        public object summary { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created_at { get; set; }
        public string status { get; set; }
        public object error { get; set; }
        public object incomplete_details { get; set; }
        public object instructions { get; set; }
        public object max_output_tokens { get; set; }
        public string model { get; set; }
        public List<Output> output { get; set; }
        public bool parallel_tool_calls { get; set; }
        public object previous_response_id { get; set; }
        public Reasoning reasoning { get; set; }
        public bool store { get; set; }
        public double temperature { get; set; }
        public Text text { get; set; }
        public string tool_choice { get; set; }
        public List<object> tools { get; set; }
        public double top_p { get; set; }
        public string truncation { get; set; }
        public Usage usage { get; set; }
        public object user { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Text
    {
        public Format format { get; set; }
    }

    public class Usage
    {
        public int input_tokens { get; set; }
        public InputTokensDetails input_tokens_details { get; set; }
        public int output_tokens { get; set; }
        public OutputTokensDetails output_tokens_details { get; set; }
        public int total_tokens { get; set; }
    }
}
