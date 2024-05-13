using Microsoft.AspNetCore.Mvc;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.Services
{
    public interface IImageProcessingService
    {
        Task<string> SendImageToDocker(byte[] imageBytes);

        Task<string> UploadToChatGPT(byte[] imageBytes, string prompt ,string apiKey);

        Task<string> UploadToCustomModel(byte[] imageBytes, string apicustomEndpoint);
    }

}