using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI_API.Images;
using WebAppBachelorProject.Controllers;
using WebAppBachelorProject.DAL.Repositories;
using WebAppBachelorProject.Data;
using WebAppBachelorProject.Models;
using WebAppBachelorProject.Services;

namespace ImageAble.Tests
{




    /*
    Explanation: 

           Setup: Initializes mocks and the controller before each test.
           Arrange: Sets up the input and mocks necessary methods or dependencies.
           Act: Calls the method under test.
           Assert: Checks the outputs and interactions are as expected.

   https://www.c-sharpcorner.com/article/introduction-to-nunit-testing-framework/

   */



    [TestFixture]
    public class ImageControllerTests
    {
        private Mock<ILogger<ImageController>> _mockLogger;
        private Mock<IImageRepository> _mockImageRepository;
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<IConfiguration> _mockConfiguration;
        private ImageController _controller;
        private Mock<IImageProcessingService> _mockImageProcessingService;


        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ImageController>>();
            _mockImageRepository = new Mock<IImageRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockImageProcessingService = new Mock<IImageProcessingService>();



            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            _mockContext = new Mock<ApplicationDbContext>(options);



            _controller = new ImageController(_mockLogger.Object, _mockImageRepository.Object, _mockContext.Object, _mockConfiguration.Object, _mockImageProcessingService.Object);
        }



        //-------------------------   GetMultipleImages -------------------------//

        //1. Checking if the request is null.
        [Test]
        public async Task GetMultipleImages_NullRequest_ReturnsBadRequestWithMessage()
        {
            //Act
            var result = await _controller.GetMultipleImages(null) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");


            var actualMethodReturn = result.Value; 
            Assert.AreEqual("Request cannot be null.", actualMethodReturn);
        }


        //2. Checking if the request.imageBase64Array is null.
        [Test]
        public async Task GetMultipleImages_ImageBase64Array_IsNull_ReturnsBadRequestWithMessage()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string>() };


            //Act
            var result = await _controller.GetMultipleImages(request) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");


            var actualMethodReturn = result.Value;
            Assert.AreEqual("Image list cannot be empty.", actualMethodReturn);
        }


        //3. Checking if request.imageBase64Array is empty.
        [Test]
        public async Task GetMultipleImages_ImageBase64Array_IsEmpty_ReturnsBadRequestWithMessage()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string>() };


            //Act
            var result = await _controller.GetMultipleImages(request) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");


            var actualMethodReturn = result.Value;
            Assert.AreEqual("Image list cannot be empty.", actualMethodReturn);
        }


        //4. Checking if imageBase64 string is invalid.
        [Test]
        public async Task GetMultipleImages_InvalidBase64String_ReturnsBadRequestWithMessage()
        {
            //Arrange
            var request = new ImageUploadRequest
            {
                ImageBase64Array = new List<string> { "Not_A_Valid_Base64_String" }
            };

            //Act
            var result = await _controller.GetMultipleImages(request) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "The result should not be null.");
            var actualMessage = result.Value;
            Assert.AreEqual("It is not a base64String", actualMessage); 
        }



        //5. Checking if description string is null.
        [Test]
        public async Task GetMultipleImages_WithNullDescription_ReturnsBadRequest()
        {
            //Arrange
            var validBase64String = "stt6gx/LzXjhTJs33+MKxg=="; 
            var request = new ImageUploadRequest
            {
                ImageBase64Array = new List<string> { validBase64String }
            };

            var methodSetup = _mockImageProcessingService.Setup(x => x.SendImageToDocker(It.IsAny<byte[]>()));
            var returnSetup = methodSetup.ReturnsAsync((string)null);


            //Act
            var result = await _controller.GetMultipleImages(request) as BadRequestObjectResult;

            //Assert
            Assert.IsNotNull(result, "Expected a non-null result for a BadRequest.");
            Assert.AreEqual("Description returned as null.", result.Value, "Expected a specific BadRequest message when the description is null.");
        }


        //6. Checking if method is successful with correct input.
        [Test]
        public async Task GetMultipleImages_Successful_ReturnsOkWithDescriptions()
        {
            //Arrange
            var validBase64String = "stt6gx/LzXjhTJs33+MKxg=="; 
            var expectedDescriptions = new List<string> { "Description1", "Description2" };
            var request = new ImageUploadRequest
            {
                ImageBase64Array = new List<string> { validBase64String, validBase64String }
            };

            _mockImageProcessingService.SetupSequence(x => x.SendImageToDocker(It.IsAny<byte[]>()))
                .ReturnsAsync("Description1")
                .ReturnsAsync("Description2");

            //Act
            var result = await _controller.GetMultipleImages(request) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);

            Assert.IsNotNull(result.Value, "The result value should not be null.");

            //Parsing the JSON object from the result.Value
            var jsonObject = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value);
            var actualResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonObject);

            Assert.IsTrue(actualResult.ContainsKey("Descriptions"), "The key 'Descriptions' was not found.");
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











        //-------------------------    CustomModelGenerator -------------------------//



    }
}
