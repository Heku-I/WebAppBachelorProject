using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using WebAppBachelorProject.DAL;
using WebAppBachelorProject.Data;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {

        private readonly ILogger<ImageController> _logger;
        private readonly IImageRepository _imageRepository;
        private readonly ApplicationDbContext _context;


        public ImageController(ILogger<ImageController> logger, IImageRepository imageRepository, ApplicationDbContext context)
        {
            _logger = logger;
            _imageRepository = imageRepository;
            _context = context;
        }



        /// <summary>
        /// This function will recieve the image from the frontend and check if image is receieved and is OK.
        /// </summary>
        /// <returns>Sends it further in the process.</returns>
        [HttpPost("GetImage")]
        public async Task<IActionResult> GetImage([FromBody] ImageDTO imageTransfer)
        {
            _logger.LogInformation("ImageController: GetImage has been called.");

            var base64Data = imageTransfer.ImageBase64.Split(',')[1]; // Removing the data:image/png;base64, part
            var imageBytes = Convert.FromBase64String(base64Data);

            _logger.LogInformation($"Image: {imageBytes}");

            var description = await SendImageToDocker(imageBytes);

            if (string.IsNullOrWhiteSpace(description))
            {
                return NotFound("The description is empty.");
            }

            // Optional: Trigger another function called GenerateEvaluation()

            return Ok(new { Description = description });
        }





        /// <summary>
        /// This function will send the image to the Docker with a REST API
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns> A generated description from the ML-model </returns>
        /// 
        //Reference: https://stackoverflow.com/questions/50670553/posting-base64-converted-image-data
        //MultipartFormDataContent //Finn sources!
        public async Task<string> SendImageToDocker(byte[] imageBytes)
        {
            _logger.LogInformation("SendImageToServer has been called.");

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    // Create ByteArrayContent from image bytes, and add to form data content
                    var imageContent = new ByteArrayContent(imageBytes);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(imageContent, "image", "upload.jpg"); 

                    var response = await client.PostAsync("http://localhost:5000/predict", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Response: {responseContent}");
                        return JsonConvert.DeserializeObject<dynamic>(responseContent).caption;
                    }
                    else
                    {
                        _logger.LogError($"Failed to get a response, status code: {response.StatusCode}");
                        return "Error: Could not get a description";
                    }
                }
            }
        }




        /// <summary>
        /// This function will be called from the GenerateDescription() (or maybe something else in the future). 
        /// It will generate the evaluation. 
        /// </summary>
        /// <returns>Return the evaluation from the description</returns>
        public Task<IActionResult> GenerateEvaluation()
        {
            _logger.LogInformation("ImageController: GenerateEvaluation has been called.");

            //Need to get the description from page.
            //Need to get the image. 

            //Need to send the image to docker ML to get the evaluation.
            //Need to retrieve the evaluation. 
            //Need to display the evaluation on page. 

            return Task.FromResult<IActionResult>(NotFound("Temporarily GenerateEvaluation")); //temp. 
        }



        /// <summary>
        /// Saving image to a folder. 
        /// User must be logged in. 
        /// </summary>
        /// <returns>The folder path</returns>
        /// 
        [Authorize]
        [HttpPost("SaveImage")]
        public async Task<IActionResult> SaveImage(IFormFile image, [FromForm] string description)
        {
            _logger.LogInformation("ImageController: SaveImage has been called.");

            if (description == null)
            {
                _logger.LogError("No description provided.");
                return BadRequest("No description provided.");
            }
            else { _logger.LogInformation($"The description is: {description} ");}

            if (image != null && image.Length > 0)
            {
                var fileName = Path.GetFileName(image.FileName);
                var path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "Uploads", fileName);

                _logger.LogInformation($"Attempting to save the image on the following path: {path}");

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream); 
                }

                return await ImageToDB(image, description, path);  
            }
            else
            {
                _logger.LogError("ImageController: Error. Image has not been saved.");
                return BadRequest(new { Success = false, Message = "Invalid image." });
            }
        }


        /// <summary>
        /// Creates a record in the Database > Image table.
        /// </summary>
        /// <returns>Success or false</returns>
        /// 
        [Authorize]
        public async Task<IActionResult> ImageToDB(IFormFile newImage, string description, string path)
        {
            _logger.LogInformation("ImageController: ImageToDB is reached.");


            Image image = new Image();

            if (newImage == null)
            {
                return BadRequest(new Responses { Success = false, Message = "Invalid image data." });

            }

            image.Description = description; 


            _logger.LogInformation("Attempting to register an image with the following model:");

            //Loggers for debugging: 
            _logger.LogInformation($"The description of the image is: {image.Description}");


            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"The save has been requested by user ID {image.UserId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("The userId is null or empty.");

                return Forbid(); //You should not be able to register an image to DB if you are not logged in. 
            }


            //Try to create the model.
            try
            {
                image.UserId = userId;
                image.ImagePath = path;

                _logger.LogInformation($"The image path of the image is: {image.ImagePath}");

                if (ModelState.IsValid)
                {
                    //Method in DAL
                    bool returnOk = await _imageRepository.Create(image);

                    if (returnOk)
                    {
                        _logger.LogInformation($"Saved image to database.\n" +
                            $"ImageId: {image.ImageId}\n" +
                            $"Description: {image.Description}\n" +
                            $"ImagePath:{image.ImagePath}\n" +
                            $"User: {image.UserId}");

                        var response = new Responses { Success = true, Message = $"Image created successfully" };
                        return Ok(response);

                    }
                    else
                    {
                        var response = new Responses { Success = false, Message = "Failure to save the record to the DB." };
                        return BadRequest(response); 
                    }
                }
                //If Modelstate is invalid.
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
                return StatusCode(500, new Responses { Success = false, Message = ex.Message });
            }
        }



        /// <summary>
        /// Deletes a image from the DB
        /// </summary>
        /// <returns>success or false</returns>
        public async Task<IActionResult> DeleteImage()
        {
            _logger.LogInformation("ImageController: DeleteImage has been called.");

            //Gets the image
            //Check authorization
            //Send to DAL (Image Repository)

            //Retrieve success or false.
            //Return success or galse.


            return NotFound("DeleteImageTemp");
        }

    }

}