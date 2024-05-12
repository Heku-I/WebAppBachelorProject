namespace WebAppBachelorProject.Services
{
    public interface IImageProcessingService
    {
        Task<string> SendImageToDocker(byte[] imageBytes);
    }

}