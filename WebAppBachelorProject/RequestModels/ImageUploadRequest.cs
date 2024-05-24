using System.Runtime.InteropServices;

namespace WebAppBachelorProject.RequestModels
{
    public class ImageUploadRequest
    {
        public List<string> ImageBase64Array { get; set; }

        public string? Prompt { get; set; }



    }
}
