using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using static OpenAI_API.Chat.ChatMessage;

namespace WebAppBachelorProject.Services
{

    public class ExternalApiService : IExternalApiService
    {
        private readonly ILogger<ExternalApiService> _logger;
        private readonly HttpClient _httpClient;



        public ExternalApiService(ILogger<ExternalApiService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }






        public async Task<string> SendImageToDockerAsync(byte[] imageBytes)
        {
            _logger.LogInformation("ExternalApiService: SendImageToDockerAsync has been called.");

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    var imageContent = new ByteArrayContent(imageBytes);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(imageContent, "image", "upload.jpg");

                    _logger.LogInformation($"ImageContent: {imageContent}");

                    var response = await client.PostAsync("http://imageabledescription.grbmhuh7fhhna7hw.swedencentral.azurecontainer.io:5000/predict", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Response: {responseContent}");
                        return JsonConvert.DeserializeObject<dynamic>(responseContent).caption;
                    }
                    else
                    {
                        _logger.LogError($"ExternalApiService: Failed to get a response, status code: {response.StatusCode}");
                        return null;
                    }
                }
            }
        }





        //https://github.com/OkGoDoIt/OpenAI-API-dotnet?tab=readme-ov-file
        //https://platform.openai.com/docs/guides/vision
        public async Task<string> UploadToChatGPTAsync(byte[] imageBytes, string prompt, string apiKey)
        {
            _logger.LogInformation("ExternalApiService: UploadToChatGPTAsync has beeøn called.");

            try
            {
                var api = new OpenAIAPI(apiKey);

                var defaultPrompt = "Create a description for the following image.";
                var promptToSend = string.IsNullOrEmpty(prompt) ? defaultPrompt : prompt;

                _logger.LogInformation($"ExternalApiService: Using prompt: {promptToSend}");

                var result = await api.Chat.CreateChatCompletionAsync(promptToSend, ImageInput.FromImageBytes(imageBytes));

                if (result == null)
                {
                    _logger.LogError("ExternalApiService: OpenAI API returned null result.");
                    return null;
                }

                var description = result.ToString();
                _logger.LogInformation($"ExternalApiService: Received description: {description}");

                return description;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExternalApiService: Exception occurred in UploadToChatGPTAsync.");
                return null;
            }
        }








        public async Task<string> UploadToCustomModelAsync(byte[] imageBytes, string customEndpoint)
        {
            _logger.LogInformation("SERVICE: UploadToCustomModelAsync has been called.");

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    var imageContent = new ByteArrayContent(imageBytes);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(imageContent, "image", "upload.jpg");

                    var response = await client.PostAsync(customEndpoint, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Response: {responseContent}");
                        return JsonConvert.DeserializeObject<dynamic>(responseContent).caption;
                    }
                    else
                    {
                        _logger.LogError($"Failed to get a response, status code: {response.StatusCode}");
                        return null;
                    }
                }
            }
        }


        public async Task<List<List<double>>> SendDescToDocker(List<string> descriptions)
        {
            _logger.LogInformation("SendDescToDocker has been called.");

            List<List<double>> allPredictions = new List<List<double>>();

            foreach (var desc in descriptions)
            {
                var requestData = new { description = desc };
                var jsonContent = JsonConvert.SerializeObject(requestData);

                using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
                {
                    var response = await _httpClient.PostAsync("http://evaluation.ajhkggevcrfqajcn.swedencentral.azurecontainer.io:5005/predict", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        var responseData = JsonConvert.DeserializeObject<JObject>(responseContent);
                        if (responseData?["predictions"] != null)
                        {
                            var predictions = responseData["predictions"].ToObject<List<List<double>>>();
                            allPredictions.AddRange(predictions);
                        }
                        else
                        {
                            _logger.LogError($"Failed to get predictions for description '{desc}' from response. Response content: {responseContent}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Failed to get a response for description '{desc}', status code: {response.StatusCode}");
                    }
                }
            }

            return allPredictions;
        }








    }

}

