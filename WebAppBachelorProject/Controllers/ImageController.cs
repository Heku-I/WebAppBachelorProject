using Microsoft.AspNetCore.Mvc;

namespace WebAppBachelorProject.Controllers
{
    public class ImageController : Controller
    {
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



            string imageId = Guid.NewGuid().ToString(); // Generate a unique ID for the image
            var tempFilePath = Path.Combine(Path.GetTempPath(), imageId + Path.GetExtension(file.FileName));

            try
            {
                // Save the file temporarily
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }


                // Return success response with imageId or a path that can be used to access the image for preview
                return Json(new { success = true, imageId = imageId });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { success = false, message = $"Error uploading image: {ex.Message}" });
            }
        }

    }
}
