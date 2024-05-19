using Microsoft.AspNetCore.Mvc;
using WebAppBachelorProject.Models;
using static WebAppBachelorProject.Services.ImageProcessingService;

namespace WebAppBachelorProject.Services
{
    public interface IImageProcessingService
    {
        //Task<string> SendImageToDocker(byte[] imageBytes);

        //Task<string> UploadToChatGPT(byte[] imageBytes, string prompt ,string apiKey);

        //Task<string> UploadToCustomModel(byte[] imageBytes, string apicustomEndpoint);

        Task<ImageProcessingResult> ProcessMultipleImagesAsync(List<string> imageBase64Array);

        Task<ImageProcessingResult> ProcessImagesWithChatGPTAsync(List<string> imageBase64Array, string prompt, string apiKey);

        Task<ImageProcessingResult> ProcessImagesWithCustomModelAsync(List<string> imageBase64Array, string customEndpoint);

        Task<IActionResult> GetEvaluation(EvaluationRequest request);



    }

}