using Microsoft.AspNetCore.Mvc;

namespace WebAppBachelorProject.Controllers
{
    public class ImageController : Controller
    {

        private readonly ILogger<ImageController> _logger;


        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
        }

        public IActionResult Generate()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Preview(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded." });
            }



            string imageId = Guid.NewGuid().ToString();
            var tempFilePath = Path.Combine(Path.GetTempPath(), imageId + Path.GetExtension(file.FileName));

            try
            {
                // Save the file temporarily
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Json(new { success = true, imageId = imageId });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { success = false, message = $"Error uploading image: {ex.Message}" });
            }
        }





        // Additional method to retrieve the image for preview, if storing path in session or cache
        public IActionResult GetImagePreview(string imageId)
        {

            var filePath = Path.Combine(Path.GetTempPath(), imageId);

            _logger.LogInformation("Filepath is " + filePath);

            if (System.IO.File.Exists(filePath))
            {
                var mimeType = "image/jpeg"; // Set appropriately based on the file type
                return PhysicalFile(filePath, mimeType);
            }

            return NotFound("Image not found.");
        }
    }

}

