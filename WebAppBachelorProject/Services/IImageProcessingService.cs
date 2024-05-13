using Microsoft.AspNetCore.Mvc;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.Services
{
    public interface IImageProcessingService
    {
        Task<string> SendImageToDocker(byte[] imageBytes);

        Task<List<string>> UploadToChatGPT(ImageUploadRequest request, string apiKey);

        Task<string> UploadToCustomModel(byte[] imageBytes, string apicustomEndpoint);
    }

}