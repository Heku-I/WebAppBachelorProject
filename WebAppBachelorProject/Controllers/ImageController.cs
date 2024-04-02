using MetadataExtractor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {

        private readonly ILogger<ImageController> _logger;


        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
        }



        /// <summary>
        /// This function will recieve the image from the frontend and check if image is receieved and is OK.
        /// </summary>
        /// <returns>Sends it further in the process.</returns>
        [HttpPost("GetImage")]
        public async Task<IActionResult> GetImage([FromBody] ImageData data)
        {
            _logger.LogInformation("ImageController: GetImage has been called.");

            var base64Data = data.Imagedata.Split(',')[1]; // Removing the data:image/png;base64, part
            var imageBytes = Convert.FromBase64String(base64Data);

            var description = await SendImageToDocker(imageBytes);

            if (string.IsNullOrWhiteSpace(description))
            {
                return NotFound("Could not generate a description.");
            }

            // Optional: Trigger another function called GenerateEvaluation()

            return Ok(new { Description = description });
        }




        /// <summary>
        /// This function will send the image to the Docker with a REST API
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns> A generated description from the ML-model </returns>
        public async Task<string> SendImageToDocker(byte[] imageBytes)
        {
            _logger.LogInformation("SendImageToDocker has been called.");



            // NEED TO IMPLEMENT A WAY TO REACH THE DOCKER CONTAINER WITH API. 



            //placeholder code
            return "Generated description from Docker container";

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
        //Needs authorization. Turned off for debugging.
        //[Authorize]
        [HttpPost("SaveImage")]
        public async Task<IActionResult> SaveImage(IFormFile image)
        {
            _logger.LogInformation("ImageController: SaveImage has been called.");

            //Need to save image to a local folder somewhere. At this point, not sure where. 
            //For now I think in "wwwroot > Uploads", unless we can use and save in some type of directory in Docker. 

            //Return folderPath + imageName

            if (image != null && image.Length > 0)
            {

                var fileName = Path.GetFileName(image.FileName);
                var path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "Uploads", fileName);

                _logger.LogInformation("Attempting to save the image on the following path:  ${path}");

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    image.CopyTo(stream);
                }


                //Extracting the metadata from the image

                _logger.LogInformation("ImageController: Calling ExtractMetaData.");

                ExtractMetaData(path);

                //XX
               

            }
            //Logging error: 
            _logger.LogError("ImageController: Error. Image has not been saved.");

            //Not sure if we need this yet...
            return Json(new { success = false, message = "Invalid file!" });

        }



        public async Task<IActionResult> ExtractMetaData(string path)
        {
            _logger.LogInformation("ImageController: ExtractMetaData is reached.");

            if (path != null)
            {

                var meta = ImageMetadataReader.ReadMetadata(path);


                foreach (var directory in meta)
                {
                    foreach (var tag in directory.Tags)
                    {
                        _logger.LogInformation($"{directory.Name} - {tag.Name} = {tag.Description}");
                    }

                    foreach (var error in directory.Errors)
                    {
                        _logger.LogError($"Error in {directory.Name}: {error}");
                    }
                }

                //Calling function to register the image in the database
                _logger.LogInformation("ImageController: Calling ImageToDB.");



                ImageToDB(); 

            }

            return Json(new { success = false, message = "Invalid file!" });


        }




       
        
        private IActionResult Json(object value)
        {
            throw new NotImplementedException();
        }




        /// <summary>
        /// Creates a record in the Database > Image table.
        /// </summary>
        /// <returns>Success or false</returns>
        public async Task<IActionResult> ImageToDB()
        {
            _logger.LogInformation("ImageController: ImageToDB is reached.");

            //Need to create the valid output.

            //Send to DAL (Image Repository)

            //Retrieve success or false.
            //Return success or galse.


            return NotFound("ImageToDbTemp");
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

