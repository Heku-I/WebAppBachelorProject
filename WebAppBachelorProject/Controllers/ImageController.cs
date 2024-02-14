using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Policy;

namespace WebAppBachelorProject.Controllers
{
    public class ImageController : Controller
    {

        private readonly ILogger<ImageController> _logger;


        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
        }




        /// <summary>
        ///  This will be the function to send the image the Docker, machine learning.
        ///  It starts when "Generate Description" button is clicked in preview-mode. 
        /// </summary>
        /// <returns>The generated description from ML algorithm</returns>
        public async Task<IActionResult> GenerateDescription()
        {

            _logger.LogInformation("ImageController: GenerateDescription has been called."); //Debug line. 

            GenerateEvaluation(); //Temp; 

            return NotFound("Temporarily GenerateDescription"); //Temp; 

            //Need to send the image to Docker. 

            //Retrieve a description from docker

            //Display the description on the page. 

            //Trigger another function called GenerateEvaluation()


            //Need to send it with JSON, image Base64.
        }


        /// <summary>
        /// This function will be called from the GenerateDescription() (or maybe something else in the future). 
        /// It will generate the evaluation. 
        /// </summary>
        /// <returns>Return the evaluation from the description</returns>
        public async Task<IActionResult> GenerateEvaluation()
        {
            _logger.LogInformation("ImageController: GenerateEvaluation has been called.");

            //Need to get the description from page.
            //Need to get the image. 

            //Need to send the image to docker ML to get the evaluation.
            //Need to retrieve the evaluation. 
            //Need to display the evaluation on page. 

            return NotFound("Temporarily GenerateEvaluation"); //temp. 
        }



        /// <summary>
        /// Saving image to a folder. 
        /// User must be logged in. 
        /// </summary>
        /// <returns>The folder path</returns>
        /// 

        [Authorize]
            public async Task<IActionResult> SaveImage(IFormFile image)
        {
            _logger.LogInformation("ImageController: SaveImage has been called.");

            //Need to save image to a local folder somewhere. At this point, not sure where. 
            //For now I think in "wwwroot > Uploads", unless we can use and save in some type of directory in Docker. 

            //Return folderPath + imageName

            if (image != null && image.Length > 0)
            {

                var fileName = Path.GetFileName(image.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                //Call ImageToDb();
                ImageToDB(); //Parameters should be the image path and metadata. 


                //Not sure if we need this yet...
                return Json(new { success = true, message = "Image uploaded successfully!" });
 
            }
            //Logging error: 
            _logger.LogError("ImageController: Error. Image has not been saved.");

            //Not sure if we need this yet...
            return Json(new { success = false, message = "Invalid file!" });
           

        }



        //    !!!!!!!!!!!!!!!!!!!!!!!!        Maybe also a function(s) to encrypt and decrypt the images??         !!!!!!!!!!!!!!!!!!!!!!!! 



        /// <summary>
        /// Creates a record in the Database > Image table.
        /// </summary>
        /// <returns>Success or false</returns>
        public async Task<IActionResult> ImageToDB()
        {
            _logger.LogInformation("ImageController: ImageToDB has been called.");

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

