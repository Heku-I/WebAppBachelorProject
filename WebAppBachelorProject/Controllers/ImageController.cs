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




        /// <summary>
        ///  This will be the function to send the image the Docker, machine learning.
        ///  It starts when "Generate Description" button is clicked in preview-mode. 
        /// </summary>
        /// <returns>The generated description from ML algorithm</returns>
        public async Task<IActionResult> GenerateDescription()
        {

            _logger.LogInformation("ImageController: GenerateDescription has been called."); //Debug line. 

            GenerateEvaluation(); //Temp; 

            return NotFound("Temporarily"); //Temp; 

            //Need to send the image to Docker. 

            //Retrieve a description from docker

            //Display the description on the page. 

            //Trigger another function called GenerateEvaluation()

        }


        /// <summary>
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

            return NotFound("Temporarily"); //temp. 
        }



        /// <summary>
        /// Saving image to a folder.
        /// </summary>
        /// <returns>The folder path</returns>
        public async Task<IActionResult> SaveImage()
        {
            _logger.LogInformation("ImageController: SaveImage has been called.");

            //Need to save image to a local folder somewhere. At this point, not sure where. 

            //Returns folderPath


            return NotFound("SaveImageTemp");
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

