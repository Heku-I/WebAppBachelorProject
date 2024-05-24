using Microsoft.AspNetCore.Mvc;
using WebAppBachelorProject.RequestModels;
using static WebAppBachelorProject.Services.ImageProcessingService;

namespace WebAppBachelorProject.Services
{
    public interface IImageProcessingService
    {
        Task<ImageProcessingResult> ProcessMultipleImagesAsync(List<string> imageBase64Array);

        Task<ImageProcessingResult> ProcessImagesWithChatGPTAsync(List<string> imageBase64Array, string prompt, string apiKey);

        Task<ImageProcessingResult> ProcessImagesWithCustomModelAsync(List<string> imageBase64Array, string customEndpoint);

        Task<IActionResult> GetEvaluation(EvaluationRequest request);

    }

}