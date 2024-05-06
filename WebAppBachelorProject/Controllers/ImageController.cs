using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using WebAppBachelorProject.DAL;
using WebAppBachelorProject.Data;
using WebAppBachelorProject.Models;
using ImageSharpImage = SixLabors.ImageSharp.Image;

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


        /*Commenting out becuase not sure if working yet:

            /// Function for the multimedia: 

            public async Task<IActionResult> GetMultipleImages ([FromBody] ImageDTO imageTransfer){

            _logger_LogInformation("ImageController: function GetMultipleImages is called"); 

            //Getting each file.
            foreach(var file in uploadfile.FormFile){
                //Checking if the file is empty.
                //Should maybe check the file format here too.
                if(file.length>0){

                var base64Data = imageTransfer.ImageBase64.Split(',')[1]; // Removing the data:image/png;base64, part
                        var imageBytes = Convert.FromBase64String(base64Data);

                    //Maybe call the SendImageToDcoker here, not sure. 
                    SendImageToDocker(imageBytes);	
                }
            }


            }


        */


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

                    var response = await client.PostAsync("http://imageable.hrf7gefrewh3e0f2.northeurope.azurecontainer.io:5000/predict", content);
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


        public class EvaluationRequest
        {
            public string Description { get; set; }
        }



        [HttpPost("GetEval")]
        public async Task<IActionResult> GetEvaluation([FromBody] EvaluationRequest request)
        {
            _logger.LogInformation(request.Description);
            _logger.LogInformation("ImageController: GetEvaluation has been called.");

            if (request != null && !string.IsNullOrWhiteSpace(request.Description))
            {
                var predictions = await SendDescToDocker(request.Description);

                if (predictions != null)
                {
                    return Ok(new { Predictions = predictions });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get predictions from Docker.");
                }
            }
            else
            {
                return BadRequest("Invalid request payload.");
            }
        }





        /// <summary>
        /// Sending the description to the docker
        /// </summary>
        /// <param name="desc"></param>
        /// <returns>Returning the evaluation</returns>
        public async Task<List<List<double>>> SendDescToDocker(string desc)
        {
            _logger.LogInformation("SendDescToDocker has been called.");

            using (var client = new HttpClient())
            {
                var requestData = new { description = desc };
                var jsonContent = JsonConvert.SerializeObject(requestData);

                using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
                {
                    var response = await client.PostAsync("http://evaluation-model.b2dqejhrezexe4gj.northeurope.azurecontainer.io:5005/predict", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Response: {responseContent}");

                        return JsonConvert.DeserializeObject<JObject>(responseContent)["predictions"].ToObject<List<List<double>>>();
                    }
                    else
                    {
                        _logger.LogError($"Failed to get a response, status code: {response.StatusCode}");
                        return null;
                    }
                }
            }
        }














        // PROPERTY TAGS (METADATA) https://learn.microsoft.com/en-gb/windows/win32/gdiplus/-gdiplus-constant-property-item-descriptions
        //USING IMAGESHARP - DOCUMENTATION https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Metadata.Profiles.Exif.html

        /// <summary>
        /// Getting request from client, function saveImage() from form.
        /// Checking if a description is provided. 
        /// Checking if image is procided.
        /// Sets the path.
        /// Detects the format on the image.
        /// Sets the description as a metadata
        /// Saves the image on the path
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="description"></param>
        /// <returns>Sends to ImageToDB() to save it to database. </returns>
        [Authorize]
        [HttpPost("SaveMeta")]
        public async Task<IActionResult> SaveMetadataToImage([FromForm] IFormFile imageFile, [FromForm] string description)
        {
            _logger.LogInformation("ImageController: SaveMetadataToImage has been called.");


            //Error logging on the required function parameter inputs.
            if (string.IsNullOrEmpty(description))
            {
                _logger.LogError("No description provided.");
                return BadRequest("No description provided.");
            }
            else
            {
                _logger.LogInformation($"Found description: {description}");
            }

            if (imageFile == null)
            {
                _logger.LogError("No image provided.");
                return BadRequest("No image provided.");
            }
            else
            {
                _logger.LogInformation("Found Image.}");

            }

            // Determine the path (wwwroot/uplads) - Open to change. 
            string fileName = Path.GetFileName(imageFile.FileName);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            string fullPath = Path.Combine(folderPath, fileName);


            try
            {
                //Accessing the file's content to read/process
                using (var imageStream = imageFile.OpenReadStream())
                {
                    //Getting the format with ImageSharp.DetectFormat. We later going to use this to save the image w/ the same format. 
                    IImageFormat format = ImageSharpImage.DetectFormat(imageStream);
                    _logger.LogInformation($"{format}"); //Debugging purposes.
                    imageStream.Position = 0;

                    //Loads the image from imageStream into an ImageSharp-object.
                    using (var image = ImageSharpImage.Load(imageStream))
                    {
                        //Getting the metadata & Inserting the description into "ImageDescription"-metadata. 
                        var metadata = image.Metadata.ExifProfile ?? new ExifProfile();
                        metadata.SetValue(ExifTag.ImageDescription, description);
                        image.Metadata.ExifProfile = metadata;

                        //Saving the image
                        await using (var outputFileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            image.Save(outputFileStream, format);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while processing the image: {ex.Message}");
            }
            _logger.LogInformation("Image has been saved successfully. Sending to ImageToDB(desc, path)");
            return await ImageToDB(description, fullPath); //For 

        }






        /* FUNCTION JUST TO SEE THE METADATA. TO BE DELTED. 
         
        public void CheckImageMetadata(string imagePath)
        {
            using (var image = ImageSharpImage.Load(imagePath))
            {
                var metadata = image.Metadata.ExifProfile;
                if (metadata != null)
                {
                    if (metadata.TryGetValue(ExifTag.ImageDescription, out var imageDescription))
                    {
                        _logger.LogInformation("Image Description from metadata: " + imageDescription.GetValue());
                    }
                    else
                    {
                        _logger.LogError("No Image Description set in EXIF data.");
                    }
                }
                else
                {
                    _logger.LogError($"No EXIF metadata found.");   
                }
            }
        }

        */







        /*
        /// <summary>
        /// Saving image to a folder. 
        /// User must be logged in. 
        /// </summary>
        /// <returns>The folder path</returns>
        ///
        [Authorize]
        public async Task<IActionResult> SaveImage(Bitmap image, string description )
        {
            _logger.LogInformation("ImageController: SaveImage has been called.");

            if (description == null)
            {
                _logger.LogError("No description provided.");
                return BadRequest("No description provided.");
            }
            else { _logger.LogInformation($"The description is: {description} ");}

            if (image != null)
            {
                // Generate a filename
                var filename = $"image_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", filename);
                _logger.LogInformation($"Attempting to save the image on the following path: {path}");

                // Save the bitmap to the path
                image.Save(path, ImageFormat.Jpeg);

                // Optionally, here you can call another function to save image details to DB
                // return await ImageToDB(image, description, path);

                return await ImageToDB(image, description, path);

            }
            else
            {
                _logger.LogError("ImageController: Error. Image has not been saved.");
                return BadRequest(new { Success = false, Message = "Invalid image." });
            }
        }
        */


        /// <summary>
        /// Creates a record in the Database > Image table.
        /// </summary>
        /// <returns>Success or false</returns>
        /// 
        [Authorize]
        public async Task<IActionResult> ImageToDB(string description, string path)
        {
            _logger.LogInformation("ImageController: ImageToDB is reached.");

            if (string.IsNullOrEmpty(path))
            {
                return BadRequest(new { Success = false, Message = "Invalid image data, path is missing." });
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"The save has been requested by user ID {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("The userId is null or empty.");

                return Forbid(); //You should not be able to register an image to DB if you are not logged in. 
            }

            Models.Image image = new Models.Image
            {
                Description = description,
                ImagePath = path,
                UserId = userId
            };


            //Loggers for debugging: 
            _logger.LogInformation($"The description of the image is: {image.Description}");
            _logger.LogInformation($"The path of the image is: {image.ImagePath}");


            //Try to create the model.
            try
            {
                if (ModelState.IsValid || image != null)
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

                        //CheckImageMetadata(path);

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
        /*public async Task<IActionResult> DeleteImage()
        {
            _logger.LogInformation("GalleryController: DeleteImage has been called.");

		

            //Gets the image
	var imageId = null;
	var imageToDelete = _context.findById(imageId);

	
            //Check authorization
	
	if(imageToDelete.User != user){
	_logger_logError("user attempted to delete image belonging to imageToDelete.User");
	return Error(error, "You are not authorized to do this deleting.");
 	
	}
            //Send to DAL (Image Repository)


	try{
		_imageRepository.remove(imageToDelete);
		logger.logInformation("Deleted the requested image from DB");

                //Need to send to another function to delete the image from the imageFolder...


            }
            catch (error e)
            {
		_logger.logError("Found error while trying to delete the requested image from DB");
		return error; 	
	}
	
	//logger.logInformation()

            //Retrieve success or false.
            //Return success or galse.


            return NotFound("DeleteImageTemp");
        }

    }
*/
    }
}