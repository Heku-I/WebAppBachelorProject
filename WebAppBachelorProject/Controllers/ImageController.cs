﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using OpenAI_API.Models;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using WebAppBachelorProject.DAL;
using WebAppBachelorProject.Data;
using WebAppBachelorProject.Models;
using static OpenAI_API.Chat.ChatMessage;
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
            List<string> descriptions = new List<string>();

            foreach (string base64String in request.ImageBase64Array)
            {
                // Convert the Base64 string to a byte array
                byte[] imageBytes = Convert.FromBase64String(base64String);

                // Pass the byte array to the ML model for prediction
                string description = await SendImageToDocker(imageBytes);
                descriptions.Add(description);
            }

            _logger.LogInformation($"Description Array: {descriptions.ToString()} ");

            return Ok(new { Descriptions = descriptions });
        }





        public async Task<List<string>> SendImagesToDockerMultiple(List<byte[]> imageBytesList)
        {
            _logger.LogInformation("SendImagesToDocker has been called.");

            List<string> descriptions = new List<string>();

            foreach (var imageBytes in imageBytesList)
            {
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
                            descriptions.Add(JsonConvert.DeserializeObject<dynamic>(responseContent).caption);
                        }
                        else
                        {
                            _logger.LogError($"Failed to get a response, status code: {response.StatusCode}");
                            descriptions.Add("Error: Could not get a description");
                        }
                    }
                }
            }

            return descriptions;
        }



        //https://github.com/OkGoDoIt/OpenAI-API-dotnet?tab=readme-ov-file
        //https://platform.openai.com/docs/guides/vision

        [HttpPost("DescFromChatGPT")]
        public async Task<IActionResult> UploadToChatGPT([FromBody]ImageUploadRequest request)
        {
            OpenAIAPI api = new OpenAIAPI("sk-proj-t0xBaRR9dj1YnFa87d3hT3BlbkFJ5DOr6j96gQJWTfEqToUA"); // shorthand
          
            List<string> descriptions = new List<string>();

            foreach(string base64String in request.ImageBase64Array)
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);

                var result = await api.Chat.CreateChatCompletionAsync("Give me a description of the following image? Make it short.",
                    ImageInput.FromImageBytes(imageBytes));

                _logger.LogInformation($"Repsonse from ChatGPT: {result}");

                descriptions.Add(result.ToString()); 
            }

            _logger.LogInformation($"Description Array: {descriptions.ToString()} ");

            return Ok(new { Descriptions = descriptions });
        }








        //Reference: https://stackoverflow.com/questions/50670553/posting-base64-converted-image-data
        //MultipartFormDataContent //Finn sources!
        /// <summary>
        /// Sends an image to a Docker-hosted service for processing and attempts to retrieve a generated caption from the response.
        /// This method utilizes a new HttpClient instance to send the image bytes as multipart/form-data to the specified endpoint.
        /// </summary>
        /// <param name="imageBytes">The byte array of the image to be sent for processing.</param>
        /// <returns>A task representing the asynchronous operation, which will return a string containing the caption of the image
        /// if the operation is successful. If the server response is not successful, it returns an error message.</returns>
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
            public List<string> description { get; set; }
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
            _logger.LogInformation("ImageController: GetEvaluation has been called.");

            if (request != null && request.description != null && request.description.Any())
            {
                var predictions = await SendDescToDocker(request.description);

                if (predictions != null)
                {
                    _logger.LogInformation("Predictions:");
                    foreach (var prediction in predictions)
                    {
                        _logger.LogInformation($"{string.Join(",", prediction)}"); // Log each prediction as a comma-separated string
                    }

                    // Return predictions to the client in a structured format
                    return Ok(predictions.SelectMany(p => p).ToList());
                }
                else
                {
                    _logger.LogError("Predictions are null or empty.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get predictions from Docker.");
                }
            }
            else
            {
                return BadRequest("Invalid request payload.");
            }
        }

        /// <summary>
        /// Sends a list of descriptions to a Docker-hosted prediction service and retrieves the predictions.
        /// This method creates a new HttpClient instance to handle the POST requests for each description,
        /// processes the responses to extract predictions, and logs the outcomes of each request.
        /// </summary>
        /// <param name="descriptions">A list of descriptions to be sent for prediction. Each description is processed independently.</param>
        /// <returns>A task representing the asynchronous operation, which will return a nested list of double values.
        /// Each inner list contains predictions for a respective description. 
        /// If any request fails, it logs an error and the corresponding prediction list might be incomplete or empty.
        /// </returns>
        public async Task<List<List<double>>> SendDescToDocker(List<string> descriptions)
        {
            _logger.LogInformation("SendDescToDocker has been called.");

            List<List<double>> allPredictions = new List<List<double>>();

            using (var client = new HttpClient())
            {
                foreach (var desc in descriptions)
                {
                    var requestData = new { description = desc };
                    var jsonContent = JsonConvert.SerializeObject(requestData);

                    using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
                    {
                        var response = await client.PostAsync("http://evaluation-model.b2dqejhrezexe4gj.northeurope.azurecontainer.io:5005/predict", content);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation($"Response for description '{desc}': {responseContent}");

                            var responseData = JsonConvert.DeserializeObject<JObject>(responseContent);
                            if (responseData?["predictions"] != null)
                            {
                                var predictions = responseData["predictions"].ToObject<List<List<double>>>();
                                allPredictions.AddRange(predictions);
                            }
                            else
                            {
                                _logger.LogError($"Failed to get predictions for description '{desc}' from response. Response content: {responseContent}");
                            }
                        }
                        else
                        {
                            _logger.LogError($"Failed to get a response for description '{desc}', status code: {response.StatusCode}");
                        }
                    }
                }
            }

            return allPredictions;
        }




        //PROPERTY TAGS (METADATA) https://learn.microsoft.com/en-gb/windows/win32/gdiplus/-gdiplus-constant-property-item-descriptions
        //USING IMAGESHARP - DOCUMENTATION https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Metadata.Profiles.Exif.html

        /// <summary>
        /// Adds metadata to an image based on the provided description and evaluation. 
        /// This method opens the image file, detects its format, and modifies its EXIF profile.
        /// </summary>
        /// <param name="imageFile">The image file from which an image stream is created and processed.</param>
        /// <param name="description">A text description that is added to the image's EXIF profile under the ImageDescription tag.</param>
        /// <param name="evaluation">An evaluation or comment that is added to the image's EXIF profile under the UserComment tag.</param>
        /// <returns>An ImageSharpImage with modified EXIF metadata, which can then be saved, downloaded, or further processed.</returns>
        private ImageSharpImage AddMetadataToImage(IFormFile imageFile, string description, string evaluation)
        {
            _logger.LogInformation("ImageController: AddMetadataToImage has been called.");

            _logger.LogInformation($"Attempting to add the following metadata: Description:{description}, Evaluation: {evaluation}");

            var imageStream = imageFile.OpenReadStream();
            var format = ImageSharpImage.DetectFormat(imageStream);
            _logger.LogInformation($"Detected format: {format.Name}");
            imageStream.Position = 0;  // Reset stream position after format detection

            var image = ImageSharpImage.Load(imageStream);

            var metadata = image.Metadata.ExifProfile ?? new ExifProfile();
            metadata.SetValue(ExifTag.ImageDescription, description);
            metadata.SetValue(ExifTag.UserComment, evaluation);
            image.Metadata.ExifProfile = metadata;

            _logger.LogInformation("Metadata is added, returning image.");

            return image;
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

            for (int i = 0; i < imageFiles.Count; i++)
            {
                var imageFile = imageFiles[i];
                var description = descriptions[i];
                var evaluation = evaluations[i];

                _logger.LogInformation($"Processing image {i}");

                string fileName = Path.GetFileName(imageFile.FileName);
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                string fullPath = Path.Combine(folderPath, fileName);

                try
                {
                    using (var image = AddMetadataToImage(imageFile, description, evaluation))
                    {
                        await using (var outputFileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            image.Save(outputFileStream, ImageSharpImage.DetectFormat(imageFile.OpenReadStream()));
                        }
                    }

                    CheckImageMetadata(fullPath);

                    _logger.LogInformation($"Image has been saved successfully. Sending to ImageToDB(desc, path, eval)");
                    await ImageToDB(description, fullPath, evaluation);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while processing image {fileName}: {ex.Message}");
                    return StatusCode(500, $"An error occurred while processing the image: {ex.Message}");
                }
            }

            return Ok(new { message = "All images have been successfully saved and processed." });
        }






















        /// <summary>
        /// Loads an image from the specified path and checks its EXIF metadata for the image description.
        /// This method is primarily used for debugging purposes to verify that metadata has been correctly embedded in an image.
        /// It logs the description if available, or logs an error if the description or EXIF metadata is missing.
        /// </summary>
        /// <param name="imagePath">The file path of the image whose metadata is to be checked.</param>
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



        /// <summary>
        /// Creates a record in the Database > Image table.
        /// </summary>
        /// <returns>Success or false</returns>
        /// 
        [Authorize]
        public async Task<IActionResult> ImageToDB(string description, string path, string evaluation)
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

            var dateCreated = DateTime.Today; 

            Models.Image image = new Models.Image
            {
                Description = description,
                ImagePath = path,
                UserId = userId,
                DateCreated = dateCreated,
                Evaluation = evaluation
                
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
                            $"DateCreated:{image.DateCreated}\n" +
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
            [FromForm(Name = "evaluation")] string evaluation
            )
        {
            _logger.LogInformation("ImageController: DownloadImageWithMetadata is reached.");

            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            try
            {
                using var imageStream = imageFile.OpenReadStream();
                var format = ImageSharpImage.DetectFormat(imageStream);  // Detect the format using the stream
                if (format == null)
                {
                    _logger.LogError("Unsupported image format.");
                    return BadRequest("Unsupported image format.");
                }

                imageStream.Position = 0;  // Reset the stream position after detecting the format

                // Use the existing function to add metadata to the image
                using var image = AddMetadataToImage(imageFile, description, evaluation);
                var memoryStream = new MemoryStream();

                image.Save(memoryStream, format);  // Save the image with metadata to memory stream
                memoryStream.Position = 0;  // Reset the position of the memory stream to enable file download

                string mimeType = format.DefaultMimeType;  // Dynamically determine the MIME type
                return File(memoryStream.ToArray(), mimeType, Path.GetFileName(imageFile.FileName));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while processing image download: {ex.Message}");
                return StatusCode(500, "Error processing your download.");
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