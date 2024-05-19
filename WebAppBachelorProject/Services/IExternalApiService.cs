namespace WebAppBachelorProject.Services
{
    public interface IExternalApiService
    {
        Task<string> SendImageToDockerAsync(byte[] imageBytes);
        Task<string> UploadToChatGPTAsync(byte[] imageBytes, string prompt, string apiKey);
        Task<string> UploadToCustomModelAsync(byte[] imageBytes, string customEndpoint);
        Task<List<List<double>>> SendDescToDocker(List<string> descriptions);


    }
}
