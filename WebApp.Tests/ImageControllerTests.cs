using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceStack;
using WebAppBachelorProject.Controllers;
using WebAppBachelorProject.DAL.Context;
using WebAppBachelorProject.Models;
using WebAppBachelorProject.Services;
using static WebAppBachelorProject.Services.ImageProcessingService;

namespace ImageAble.Tests
{


    /*
    Explanation: 
            SOURCES:     https://www.c-sharpcorner.com/article/introduction-to-nunit-testing-framework/

           Setup: Initializes mocks and the controller before each test.

           Arrange: Sets up the input and mocks necessary methods or dependencies.

           Act: Calls the method under test.

           Assert: Checks the outputs and interactions are as expected.



   */



    [TestFixture]
    public class ImageControllerTests
    {
        private Mock<ILogger<ImageController>> _mockLogger;
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<IConfiguration> _mockConfiguration;
        private ImageController _controller;
        private Mock<IImageProcessingService> _mockImageProcessingService;
        private Mock<IImageService> _mockImageService;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ImageController>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockImageProcessingService = new Mock<IImageProcessingService>();
            _mockImageService = new Mock<IImageService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            _mockContext = new Mock<ApplicationDbContext>(options);

            _controller = new ImageController(
                _mockLogger.Object,
                _mockContext.Object,
                _mockConfiguration.Object,
                _mockImageProcessingService.Object,
                _mockImageService.Object
            );
        }
        public class GetMultipleImagesResponse
        {
            public List<string> Descriptions { get; set; }
        }

        //------------------------- Function: GetMultipleImages -------------------------//

        //1. Checking if the request is null.
        [Test]
        public async Task GetMultipleImages_RequestIsNull_ReturnsBadRequest()
        {
            //Act
            var result = await _controller.GetMultipleImages(null);

            //Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }



        //2. Checking if the request.ImageBase64Array is null.
        [Test]
        public async Task GetMultipleImages_ImageBase64ArrayIsNull_ReturnsBadRequest()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = null };
            //Act
            var result = await _controller.GetMultipleImages(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }



        //3. Checking if request.ImageBase64Array is empty.
        [Test]
        public async Task GetMultipleImages_ImageBase64Array_IsEmpty_ReturnsBadRequestWithMessage()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string>() };


            //Act
            var result = await _controller.GetMultipleImages(request) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result, "The result should not be null.");
            Assert.AreEqual("Invalid request.", result.Value);
        }



        //4. Checking if the image processing service fails.
        [Test]
        public async Task GetMultipleImages_ImageProcessingServiceFails_ReturnsBadRequest()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "testImage" } };
            _mockImageProcessingService
                .Setup(service => service.ProcessMultipleImagesAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new ImageProcessingResult { Success = false, Message = "Error processing images" });
            //Act

            var result = await _controller.GetMultipleImages(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);

            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Error processing images", badRequestResult.Value);
        }



        //5. Checking if the image processing service succeeds.
        [Test]
        public async Task GetMultipleImages_Successful_ReturnsOkWithDescriptions()
        {
            // Arrange
            var validBase64String = "stt6gx/LzXjhTJs33+MKxg=="; //rndm base64-string
            // Settign what we desire to return. 
            var expectedDescriptions = new List<string> { "Description1", "Description2" }; 

            //Creating a valid request (from the parameter)
            var request = new ImageUploadRequest
            {
                ImageBase64Array = new List<string> { validBase64String, validBase64String }
            };

            //Mocking the success from service
            _mockImageProcessingService
                .Setup(service => service.ProcessMultipleImagesAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new ImageProcessingResult
                {
                    Success = true,
                    Descriptions = expectedDescriptions
                });

            // Act
            var result = await _controller.GetMultipleImages(request) as OkObjectResult;


            // Assert

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNotNull(result.Value, "The result value should not be null.");

            //JSON parsing becuase of anonymous type. We convert it into string format so we can parse.
            var jsonObject = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value);

            var actualResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonObject);

            Assert.IsTrue(actualResult.ContainsKey("Descriptions"));
            var descriptions = actualResult["Descriptions"];
            Assert.IsNotNull(descriptions, "Descriptions should not be null.");
            Assert.AreEqual(2, descriptions.Count, "There should be two descriptions.");
            CollectionAssert.AreEqual(expectedDescriptions, descriptions, "Descriptions should match expected values.");
        }




        //-------------------------    UploadToChatGPT -------------------------//

        //7. Checking if the request is null.
        [Test]
        public async Task UploadToChatGPT_NullRequest_ReturnsBadRequestWithMessage()
        {
            var apikey = "apikey";

            //Act
            var result = await _controller.UploadToChatGPT(null, apikey) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");


            var actualMethodReturn = result.Value;
            Assert.AreEqual("Request cannot be null.", actualMethodReturn);
        }


        //8. Checking if the request.imageBase64Array is null.
        [Test]
        public async Task UploadToChatGPT_ImageBase64Array_IsNull_ReturnsBadRequestWithMessage()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string>() };
            var apikey = "apikey"; 


            //Act
            var result = await _controller.UploadToChatGPT(request, apikey) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");


            var actualMethodReturn = result.Value;
            Assert.AreEqual("Image list cannot be empty.", actualMethodReturn);
        }


        //9. Checking if request.imageBase64Array is empty.
        [Test]
        public async Task UploadToChatGPT_ImageBase64Array_IsEmpty_ReturnsBadRequestWithMessage()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string>() };
            var apikey = "apiKey";


            //Act
            var result = await _controller.UploadToChatGPT(request, apikey) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");


            var actualMethodReturn = result.Value;
            Assert.AreEqual("Image list cannot be empty.", actualMethodReturn);
        }


        //10. Checking if imageBase64 string is invalid.
        [Test]
        public async Task UploadToChatGPT_InvalidBase64String_ReturnsBadRequestWithMessage()
        {
            var apikey = "apiKey";
            //Arrange
            var request = new ImageUploadRequest
            {
                ImageBase64Array = new List<string> { "Not_A_Valid_Base64_String" }
            };

            //Act
            var result = await _controller.UploadToChatGPT(request, apikey) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");
            var actualMessage = result.Value;
            Assert.AreEqual("It is not a base64String", actualMessage);
        }


        //11. Checking if API key is null
        [Test]
        public async Task UploadToChatGPTs_apiKey_IsNull_ReturnsBadRequestWithMessage()
        {
            //Arrange

            var validBase64String = "stt6gx/LzXjhTJs33+MKxg==";

            var request = new ImageUploadRequest
            {
                ImageBase64Array = new List<string> { validBase64String, validBase64String }
            };

            //Act
            var result = await _controller.UploadToChatGPT(request, null) as BadRequestObjectResult;

            //AssertVal
            Assert.IsNotNull(result, "The result should not be null.");


            var actualMethodReturn = result.Value;
            Assert.AreEqual("API-key is necessary. Please enter a API-key", actualMethodReturn);
        }





        /*
        //12. Checking if method is successful with correct input.
        [Test]
        public async Task UploadToChatGPT_Successful_ReturnsOkWithDescriptions()
        {
            // Arrange
            var apiKey = "apiKey";
            var validBase64String = "stt6gx/LzXjhTJs33+MKxg==";
            var expectedDescriptions = new List<string> { "Description1", "Description2" };
            var request = new ImageUploadRequest
            {
                ImageBase64Array = new List<string> { validBase64String, validBase64String },
                Prompt = "Test prompt"
            };

            _mockImageProcessingService.SetupSequence(x => x.UploadToChatGPT(It.IsAny<byte[]>(), It.IsAny<string>(), apiKey))
                .ReturnsAsync("Description1")
                .ReturnsAsync("Description2");

            // Act
            var result = await _controller.UploadToChatGPT(request, apiKey) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);

            Assert.IsNotNull(result.Value, "The result value should not be null.");

            // Parsing the JSON object from the result.Value
            var jsonObject = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value);
            var actualResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonObject);

            Assert.IsTrue(actualResult.ContainsKey("Descriptions"), "The key 'Descriptions' was not found.");
            var descriptions = actualResult["Descriptions"];
            Assert.IsNotNull(descriptions, "Descriptions should not be null.");
            Assert.AreEqual(2, descriptions.Count, "There should be two descriptions.");
            CollectionAssert.AreEqual(expectedDescriptions, descriptions, "Descriptions should match expected values.");
        }


        */








        //-------------------------    CustomModelGenerator -------------------------//



    }
}
