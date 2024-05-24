using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI_API;
using System.Net.Http.Headers;
using static OpenAI_API.Chat.ChatMessage;
using WebAppBachelorProject.RequestModels;


namespace WebAppBachelorProject.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ILogger<ImageProcessingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IExternalApiService _externalApiService;


        public ImageProcessingService(ILogger<ImageProcessingService> logger, IConfiguration configuration, IExternalApiService externalApiService)
        {
            _logger = logger;
            _configuration = configuration;
            _externalApiService = externalApiService;
        }


        //https://stackoverflow.com/questions/6309379/how-to-check-for-a-valid-base64-encoded-string/54143400#54143400
        //Checking if it is a base64.
        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int _);
        }



        public class ImageProcessingResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<string> Descriptions { get; set; }
        }



        //Reference: https://stackoverflow.com/questions/50670553/posting-base64-converted-image-data
        //MultipartFormDataContent //Finn sources!
        public async Task<ImageProcessingResult> ProcessMultipleImagesAsync(List<string> imageBase64Array)
        {
            _logger.LogInformation("ImageProcessingService: ProcessMultipleImagesAsync has been called.");

            var result = new ImageProcessingResult { Success = true, Descriptions = new List<string>() };

            foreach (var base64String in imageBase64Array)
            {
                if (!IsBase64String(base64String))
                {
                    result.Success = false;
                    result.Message = "One or more strings are not valid Base64.";
                    return result;
                }

                try
                {
                    byte[] imageBytes = Convert.FromBase64String(base64String);
                    string description = await _externalApiService.SendImageToDockerAsync(imageBytes);

                    if (description == null)
                    {
                        result.Success = false;
                        result.Message = "Failed to get description from Docker service.";
                        return result;
                    }

                    result.Descriptions.Add(description);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image");
                    result.Success = false;
                    result.Message = $"Error processing images: {ex.Message}";
                    return result;
                }
            }

            return result;
        }



        public async Task<ImageProcessingResult> ProcessImagesWithChatGPTAsync(List<string> imageBase64Array, string prompt, string apiKey)
        {
            var result = new ImageProcessingResult { Success = true, Descriptions = new List<string>() };

            foreach (var base64String in imageBase64Array)
            {
                if (!IsBase64String(base64String))
                {
                    result.Success = false;
                    result.Message = "One or more strings are not valid Base64.";
                    return result;
                }

                try
                {
                    byte[] imageBytes = Convert.FromBase64String(base64String);
                    string description = await _externalApiService.UploadToChatGPTAsync(imageBytes, prompt, apiKey);

                    if (description == null)
                    {
                        result.Success = false;
                        result.Message = "Failed to get description from ChatGPT.";
                        return result;
                    }

                    result.Descriptions.Add(description);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image with ChatGPT");
                    result.Success = false;
                    result.Message = $"Error processing images with ChatGPT: {ex.Message}";
                    return result;
                }
            }

            return result;
        }



        public async Task<ImageProcessingResult> ProcessImagesWithCustomModelAsync(List<string> imageBase64Array, string customEndpoint)
        {
            var result = new ImageProcessingResult { Success = true, Descriptions = new List<string>() };

            foreach (var base64String in imageBase64Array)
            {
                if (!IsBase64String(base64String))
                {
                    result.Success = false;
                    result.Message = "One or more strings are not valid Base64.";
                    return result;
                }

                try
                {
                    byte[] imageBytes = Convert.FromBase64String(base64String);
                    string description = await _externalApiService.UploadToCustomModelAsync(imageBytes, customEndpoint);

                    if (description == null)
                    {
                        result.Success = false;
                        result.Message = "Failed to get description from custom model.";
                        return result;
                    }

                    result.Descriptions.Add(description);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image with custom model");
                    result.Success = false;
                    result.Message = $"Error processing images with custom model: {ex.Message}";
                    return result;
                }
            }

            return result;
        }



        public async Task<IActionResult> GetEvaluation(EvaluationRequest request)
        {
            _logger.LogInformation("ImageProcessingService: GetEvaluation has been called.");

            if (request != null && request.description != null && request.description.Any())
            {
                var predictions = await _externalApiService.SendDescToDocker(request.description);

                if (predictions != null)
                {
                    // Return predictions to the client in a structured format
                    return new OkObjectResult(predictions.SelectMany(p => p).ToList());
                }
                else
                {
                    _logger.LogError("Predictions are null or empty.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                return new BadRequestObjectResult("Invalid request payload.");
            }
        }









    }
}
