using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAppBachelorProject.DAL.Context;
using WebAppBachelorProject.DAL.Repositories;
using WebAppBachelorProject.Models;
using WebAppBachelorProject.Services;

namespace WebAppBachelorProject.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {

        private readonly ILogger<ImageController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IImageService _imageService;




        public ImageController(ILogger<ImageController> logger,
            ApplicationDbContext context, IConfiguration configuration, 
            IImageProcessingService imageProcessingService, 
            IImageService imageService)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _imageProcessingService = imageProcessingService;
            _imageService = imageService;


        }


        /// <summary>
        /// Processes multiple image uploads provided as Base64-encoded strings within the request body,
        /// converts them to byte arrays, and sends each image to a Docker-hosted service for description prediction.
        /// This method logs the descriptions of all processed images and returns them in a structured response.
        /// </summary>
        /// <param name="request">The request containing an array of Base64-encoded strings representing images.</param>
        /// <returns>A task representing the asynchronous operation, which returns an IActionResult that encapsulates the descriptions of all images processed.
        /// If the process is successful, it returns an OK status with the descriptions; otherwise, 
        /// it handles potential errors in image processing or prediction internally.
        /// </returns>

        [HttpPost("GetMultipleImages")]
        public async Task<IActionResult> GetMultipleImages([FromBody] ImageUploadRequest request)
        {
            _logger.LogInformation("ImageController: GetMultipleImages has been called.");

            if (request == null || request.ImageBase64Array == null || request.ImageBase64Array.Count == 0)
            {
                return BadRequest("Invalid request.");
            }
            var result = await _imageProcessingService.ProcessMultipleImagesAsync(request.ImageBase64Array);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(new { Descriptions = result.Descriptions });
        }



        [HttpPost("DescFromChatGPT")]
        public async Task<IActionResult> UploadToChatGPT([FromBody] ImageUploadRequest request, [FromHeader] string apiKey)
        {
            if (request == null || request.ImageBase64Array == null || request.ImageBase64Array.Count == 0)
            {
                return BadRequest("Invalid request.");
            }
            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("API key is necessary. Please enter an API key.");
            }
            var result = await _imageProcessingService.ProcessImagesWithChatGPTAsync(request.ImageBase64Array, request.Prompt, apiKey);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(new { Descriptions = result.Descriptions });
        }


        [HttpPost("Custom")]
        public async Task<IActionResult> CustomModelGenerator([FromBody] ImageUploadRequest request, [FromHeader] string customEndpoint)
        {
            if (request == null || request.ImageBase64Array == null || request.ImageBase64Array.Count == 0)
            {
                return BadRequest("Invalid request.");
            }

            if (string.IsNullOrEmpty(customEndpoint))
            {
                return BadRequest("Endpoint URL is necessary. Please enter an Endpoint URL.");
            }

            var result = await _imageProcessingService.ProcessImagesWithCustomModelAsync(request.ImageBase64Array, customEndpoint);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(new { Descriptions = result.Descriptions });
        }



        /// <summary>
        /// Receives an evaluation request and processes it to return predictions. This method logs the incoming request,
        /// sends it to a Docker-based service for prediction, and then returns the predictions as a response.
        /// </summary>
        /// <param name="request">The evaluation request containing a description that is sent to a Docker service for processing.</param>
        /// <returns>A task representing the asynchronous operation which will return an IActionResult. If predictions are successful,
        /// it returns a list of predictions. If the predictions are null or the request is invalid, it returns an appropriate error status.</returns>
        [HttpPost("GetEval")]
        public async Task<IActionResult> GetEvaluation([FromBody] EvaluationRequest request)
        {

            if (request == null || request.description == null || request.description.Count == 0)
            {
                return BadRequest("Invalid request.");
            }

            _logger.LogInformation("ImageController: GetEvaluation has been called.");
            return await _imageProcessingService.GetEvaluation(request);
        }









        /// <summary>
        /// Asynchronously saves a list of images to the server folder.
        /// This endpoint requires authorization and handles POST requests to "SaveImage".
        /// </summary>
        /// <param name="imageFiles">The list of image files to be saved. These files are passed as form data named "imageFiles".</param>
        /// <param name="descriptions">Descriptions for each image file. Passed as form data named "descriptions"</param>
        /// <param name="evaluations">Evaluations for each description. Passed as form data named "evaluations"</param>
        /// <returns>indication of success or failure of the image saving process.</returns>
        [Authorize]
        [HttpPost("SaveImage")]
        public async Task<IActionResult> SaveImageToFolder(
            [FromForm(Name = "imageFiles")] List<IFormFile> imageFiles,
            [FromForm(Name = "descriptions")] List<string> descriptions,
            [FromForm(Name = "evaluations")] List<string> evaluations)
        {
            _logger.LogInformation("ImageController: SaveImageToFolder has been called.");

            if (imageFiles == null || imageFiles.Count == 0)
            {
                return BadRequest("Invalid request.");
            }

            if (descriptions == null || descriptions.Count == 0)
            {
                return BadRequest("Invalid request.");
            }

            if (evaluations == null || evaluations.Count == 0)
            {
                return BadRequest("Invalid request.");
            }

            if (evaluations.Count != descriptions.Count || evaluations.Count != imageFiles.Count || descriptions.Count != imageFiles.Count)
            {
                return BadRequest("Invalid request.");
            }


            for (int i = 0; i < imageFiles.Count; i++)
            {
                try
                {
                    await _imageService.SaveImageToFolderAsync(imageFiles[i], descriptions[i], evaluations[i], i);
                    await SaveImageToDB(descriptions[i], imageFiles[i].FileName, evaluations[i]);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while processing image {imageFiles[i].FileName}: {ex.Message}");
                    return StatusCode(500, $"An error occurred while processing the image: {ex.Message}");
                }
            }

            return Ok(new { message = "All images have been successfully saved and processed." });
        }





        //PROPERTY TAGS (METADATA) https://learn.microsoft.com/en-gb/windows/win32/gdiplus/-gdiplus-constant-property-item-descriptions
        //USING IMAGESHARP - DOCUMENTATION https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Metadata.Profiles.Exif.html

        [Authorize]
        private async Task<IActionResult> SaveImageToDB(string description, string fileName, string evaluation)
        {
            _logger.LogInformation("ImageController: SaveImageToDB is reached.");

            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new { Success = false, Message = "Invalid image data, path is missing." });
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"The save has been requested by user ID {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("The userId is null or empty.");
                return Forbid();
            }

            //Creating a relative path so we can easily use it in the client side to get the picture out as well. 
            string relativePath = $"/Uploads/{fileName}";
            var dateCreated = DateTime.Today;

            Models.Image image = new Models.Image
            {
                Description = description,
                ImagePath = relativePath,
                UserId = userId,
                DateCreated = dateCreated,
                Evaluation = evaluation
            };

            _logger.LogInformation($"The description of the image is: {image.Description}");
            _logger.LogInformation($"The path of the image is: {image.ImagePath}");

            try
            {
                if (ModelState.IsValid || image != null)
                {
                    bool result = await _imageService.SaveImageToDBAsync(image);

                    if (result)
                    {
                        _logger.LogInformation($"Saved image to database.\n" +
                            $"ImageId: {image.ImageId}\n" +
                            $"Description: {image.Description}\n" +
                            $"ImagePath:{image.ImagePath}\n" +
                            $"DateCreated:{image.DateCreated}\n" +
                            $"User: {image.UserId}");

                        var response = new { Success = true, Message = "Image created successfully" };
                        return Ok(response);
                    }
                    else
                    {
                        var response = new { Success = false, Message = "Failure to save the record to the DB." };
                        return BadRequest(response);
                    }
                }
                else
                {
                    _logger.LogError("ModelState is invalid");
                    var response = new { Success = false, Message = "Model state is invalid", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) };
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the image.");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }



        /// <summary>
        /// Processes an uploaded image file by adding metadata the following metadata: description and evaluation of the description. 
        /// and returns the modified image as a downloadable file.
        /// </summary>
        /// <param name="imageFile">The image file uploaded by the user. Should contain image data. Should not be null.</param>
        /// <param name="description">The description to be added into the image's metadata. Provides additional context to the image.</param>
        /// <param name="evaluation">The evaluation of the description to be added into the image's metadata. Provides trust if the description is good.</param>
        /// <returns>Returns a FileContentResult containing the modified image with metadata as a downloadable file. 
        /// If an error occurs, it returns an appropriate HTTP status code with error details.
        /// </returns>
        [HttpPost("downloadImageWithMetadata")]
        public async Task<IActionResult> DownloadImageWithMetadata(
        [FromForm(Name = "imageFile")] IFormFile imageFile,
        [FromForm(Name = "description")] string description,
        [FromForm(Name = "evaluation")] string evaluation)
        {
            _logger.LogInformation("ImageController: DownloadImageWithMetadata is reached.");

            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            if (description == null || description.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            if (evaluation == null || evaluation.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            try
            {
                var result = await _imageService.DownloadImageWithMetadataAsync(imageFile, description, evaluation);
                return result;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while processing image download: {ex.Message}");
                return StatusCode(500, "Error processing your download.");
            }
        }



    }
}