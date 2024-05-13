using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI_API;
using System.Net.Http.Headers;
using static OpenAI_API.Chat.ChatMessage;
using WebAppBachelorProject.Models;


namespace WebAppBachelorProject.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ILogger<ImageProcessingService> _logger;
        private readonly IConfiguration _configuration;

        public ImageProcessingService(ILogger<ImageProcessingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }


        //Reference: https://stackoverflow.com/questions/50670553/posting-base64-converted-image-data
        //MultipartFormDataContent //Finn sources!
        /// <summary>
        /// Sends an image to a Docker-hosted service for processing and attempts to retrieve a generated caption from the response.
        /// This method utilizes a new HttpClient instance to send the image bytes as multipart/form-data to the specified endpoint.
        /// </summary>
        /// <param name="imageBytes">The byte array of the image to be sent for processing.</param>
        /// <returns>A task representing the asynchronous operation, which will return a string containing the caption of the image
        /// if the operation is successful. If the server response is not successful, it returns an error message.</returns>
        public async Task<string> SendImageToDocker(byte[] imageBytes)
        {
            _logger.LogInformation("SendImageToServer has been called.");

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    //Create ByteArrayContent from image bytes, and add to form data content
                    var imageContent = new ByteArrayContent(imageBytes);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(imageContent, "image", "upload.jpg");

                    var response = await client.PostAsync("http://imageable.hrf7gefrewh3e0f2.northeurope.azurecontainer.io:5000/predict", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Response: {responseContent}");
                        return JsonConvert.DeserializeObject<dynamic>(responseContent).caption;
                    }
                    else
                    {
                        _logger.LogError($"Failed to get a response, status code: {response.StatusCode}");
                        return "Error: Could not get a description";
                    }
                }
            }
        }



        //https://github.com/OkGoDoIt/OpenAI-API-dotnet?tab=readme-ov-file
        //https://platform.openai.com/docs/guides/vision
        public async Task<List<string>> UploadToChatGPT(ImageUploadRequest request, string apiKey)
        {
            OpenAIAPI api = new OpenAIAPI(apiKey);
            List<string> descriptions = new List<string>();

            var defaultPrompt = "Create a description for the following image.";

            var promptToSend = ""; 

            if (request.Prompt == null)
            {
                promptToSend = defaultPrompt;
            }
            else
            {
                promptToSend = request.Prompt;
            }

            foreach (string base64String in request.ImageBase64Array)
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                var result = await api.Chat.CreateChatCompletionAsync(promptToSend, ImageInput.FromImageBytes(imageBytes));

                _logger.LogInformation($"Response from ChatGPT: {result}");
                descriptions.Add(result.ToString());
            }

            _logger.LogInformation($"Description Array: {descriptions}");
            return descriptions;
        }












        /* MAYBE REMOVE. 

public async Task<List<string>> SendImagesToDockerMultiple(List<byte[]> imageBytesList)
{
    _logger.LogInformation("SendImagesToDocker has been called.");

    List<string> descriptions = new List<string>();

    foreach (var imageBytes in imageBytesList)
    {
        using (var client = new HttpClient())
        {
            using (var content = new MultipartFormDataContent())
            {
                // Create ByteArrayContent from image bytes, and add to form data content
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(imageContent, "image", "upload.jpg");

                var response = await client.PostAsync("http://localhost:5000/predict", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response: {responseContent}");
                    descriptions.Add(JsonConvert.DeserializeObject<dynamic>(responseContent).caption);
                }
                else
                {
                    _logger.LogError($"Failed to get a response, status code: {response.StatusCode}");
                    descriptions.Add("Error: Could not get a description");
                }
            }
        }
    }

    return descriptions;
}

*/






    }
}
