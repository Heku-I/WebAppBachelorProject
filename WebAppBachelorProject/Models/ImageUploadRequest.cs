using System.Runtime.InteropServices;

namespace WebAppBachelorProject.Models
{
    public class ImageUploadRequest
    {
        public List<string> ImageBase64Array { get; set; }

        public string? Prompt { get; set; } 



    }
}
