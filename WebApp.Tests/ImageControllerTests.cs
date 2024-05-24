using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceStack;
using WebAppBachelorProject.Controllers;
using WebAppBachelorProject.DAL.Context;
using WebAppBachelorProject.Models;
using WebAppBachelorProject.RequestModels;
using WebAppBachelorProject.Services;
using static WebAppBachelorProject.Services.ImageProcessingService;

namespace WebApp.Tests
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

        //6. Checking if the request is null.
        [Test]
        public async Task UploadToChatGPT_RequestIsNull_ReturnsBadRequest()
        {
            //Act
            var result = await _controller.UploadToChatGPT(null, "123123123");

            //Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }



        //7. Checking if the request.ImageBase64Array is null.
        [Test]
        public async Task UploadToChatGPT_ImageBase64ArrayIsNull_ReturnsBadRequest()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = null };

            //Act
            var result = await _controller.UploadToChatGPT(request, "123123123");

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }



        //8. Checking if request.ImageBase64Array is empty.
        [Test]
        public async Task UploadToChatGPT_ImageBase64ArrayIsEmpty_ReturnsBadRequest()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string>() };


            //Act
            var result = await _controller.UploadToChatGPT(request, "123123123");

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //9. Chekcing if Api-key is null. 
        [Test]
        public async Task UploadToChatGPT_ApiKeyIsNull_ReturnsBadRequest()
        {
            //Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "stt6gx/LzXjhTJs33+MKxg" } };

            //Act
            var result = await _controller.UploadToChatGPT(request, null);

            //Assert

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }


        //10 Checking if the apikey is empty
        [Test]
        public async Task UploadToChatGPT_ApiKeyIsEmpty_ReturnsBadRequest()
        {
            //Arrange

            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "stt6gx/LzXjhTJs33+MKxg==" } };

            //Act
            var result = await _controller.UploadToChatGPT(request, string.Empty);
            //Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }


        //11. Checking if we can get a successful response (OK). 
        [Test]
        public async Task UploadToChatGPT_ImageProcessingServiceSucceeds_ReturnsOk()
        {
            // Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "stt6gx/LzXjhTJs33+MKxg==" } }; //Setting up a mocked ImageUploadRequest.
            var expectedDescriptions = new List<string> { "Description1" }; //Settiping up the expected Description which will be retreieved from the service.

            _mockImageProcessingService //mocking the uploadToChatGPT service to return a successful ImageProcessingResult w/ expected desc.
                .Setup(service => service.ProcessImagesWithChatGPTAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ImageProcessingResult
                {
                    Success = true,
                    Descriptions = expectedDescriptions
                });

            // Act
            var result = await _controller.UploadToChatGPT(request, "validAPIKEY") as OkObjectResult; //Calling the func.

            // Assert
            Assert.IsNotNull(result); //Checking that it is not null. 
            Assert.IsInstanceOf<OkObjectResult>(result); //Checking the type
            Assert.IsNotNull(result.Value);

            //Send the result value to the helper class (GetMultipleImagesRepsonse) and then using 
            //Serialize and Deserializing JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value);
            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<GetMultipleImagesResponse>(json); Assert.IsNotNull(value);

            //Checking if the value.Description is not null. and then if it is equal to the expected.
            Assert.IsNotNull(value.Descriptions);
            Assert.AreEqual(expectedDescriptions.Count, value.Descriptions.Count);
            CollectionAssert.AreEqual(expectedDescriptions, value.Descriptions);
        }





        //-------------------------    CustomModelGenerator -------------------------//

        //12. Checking if request is null. 
        [Test]
        public async Task CustomModelGenerator_RequestIsNull_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CustomModelGenerator(null, "https://somethingsomething.com");

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }


        //13. Checking if imageBase64Array is null. 
        [Test]
        public async Task CustomModelGenerator_ImageBase64ArrayIsNull_ReturnsBadRequest()
        {
            // Arrange
            var request = new ImageUploadRequest { ImageBase64Array = null }; //Where Imagebase64Array = null. 

            // Act
            var result = await _controller.CustomModelGenerator(request, "https://somethingsomething.com"); //Random endpoint
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result); //Does it result in a BadRequest?
        }


        //14. Checking if imageBase64Array is empty. 
        [Test]
        public async Task CustomModelGenerator_ImageBase64ArrayIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string>() };
            // Act
            var result = await _controller.CustomModelGenerator(request, "https://somethingsomething.com");

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //15. Checking if customEndpoint is null. 
        [Test]
        public async Task CustomModelGenerator_CustomEndpointIsNull_ReturnsBadRequest()
        {
            // Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "tt6gx/LzXjhTJs33+MKxg==" } };

            // Act
            var result = await _controller.CustomModelGenerator(request, null);


            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //16. Checking if customEndpoint is empty. 
        [Test]
        public async Task CustomModelGenerator_CustomEndpointIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "tt6gx/LzXjhTJs33+MKxg==" } };


            // Act
            var result = await _controller.CustomModelGenerator(request, string.Empty);


            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //16. Checking if the service success = false. 
        [Test]
        public async Task CustomModelGenerator_ImageProcessingServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "tt6gx/LzXjhTJs33+MKxg==" } };
            _mockImageProcessingService
                .Setup(service => service.ProcessImagesWithCustomModelAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
                .ReturnsAsync(new ImageProcessingResult
                {
                    Success = false,
                    Message = "Error processing images"
                }
                );

            // Act
            var result = await _controller.CustomModelGenerator(request, "https://somethingsomething.com");
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Error processing images", badRequestResult.Value);
        }



        //17. checking if the function runs successfully and returns OK. 
        [Test]
        public async Task CustomModelGenerator_ImageProcessingServiceSucceeds_ReturnsOk()
        {
            // Arrange
            var request = new ImageUploadRequest { ImageBase64Array = new List<string> { "tt6gx/LzXjhTJs33+MKxg==" } }; //Setting up a imageBase64 array.
            var expectedDescriptions = new List<string> { "Description1" }; //Setting up the expected desc.

            //Setting up a mock where we return success = true. 
            _mockImageProcessingService
                .Setup(service => service.ProcessImagesWithCustomModelAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
                .ReturnsAsync(new ImageProcessingResult
                {
                    Success = true,
                    Descriptions = expectedDescriptions
                });

            // Act
            var result = await _controller.CustomModelGenerator(request, "https://somethingsomething.com") as OkObjectResult; //Running the controller with the mock.

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNotNull(result.Value);

            //Serialize & Deserialize gettong JSON in string to identify/read the object. 
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value);
            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<GetMultipleImagesResponse>(json);

            Assert.IsNotNull(value);
            Assert.IsNotNull(value.Descriptions);
            Assert.AreEqual(expectedDescriptions.Count, value.Descriptions.Count);
            CollectionAssert.AreEqual(expectedDescriptions, value.Descriptions);
        }


        //-------------------------    GetEvaluation   -------------------------//

        //18 checking if request is null. 
        [Test]
        public async Task GetEvaluation_RequestIsNull_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.GetEvaluation(null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Invalid request.", badRequestResult.Value);
        }

        //19. checking if request.description is null. 
        [Test]
        public async Task GetEvaluation_DescriptionIsNull_ReturnsBadRequest()
        {
            // Arrange
            var request = new EvaluationRequest { description = null };

            // Act
            var result = await _controller.GetEvaluation(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Invalid request.", badRequestResult.Value);
        }

        //20 checking if request.description is empty 
        [Test]
        public async Task GetEvaluation_DescriptionIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var request = new EvaluationRequest { description = new List<string>() };

            // Act
            var result = await _controller.GetEvaluation(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Invalid request.", badRequestResult.Value);
        }



        //21. Testing failure form the service.
        [Test]
        public async Task GetEvaluation_ImageProcessingServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var request = new EvaluationRequest { description = new List<string> { "testDescription" } };
            _mockImageProcessingService
                .Setup(service => service.GetEvaluation(It.IsAny<EvaluationRequest>()))
                .ReturnsAsync(new BadRequestObjectResult("Error processing evaluation"));

            // Act
            var result = await _controller.GetEvaluation(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Error processing evaluation", badRequestResult.Value);
        }
        
        //22. Testing successfull response from the function. 
        [Test]
        public async Task GetEvaluation_ImageProcessingServiceSucceeds_ReturnsOk()
        {
            // Arrange
            var request = new EvaluationRequest { description = new List<string> { "testDescription" } };
            var expectedData = new { Result = "Evaluation result" }; // Replace with appropriate expected data
            _mockImageProcessingService
                .Setup(service => service.GetEvaluation(It.IsAny<EvaluationRequest>()))
                .ReturnsAsync(new OkObjectResult(expectedData));

            // Act
            var result = await _controller.GetEvaluation(request) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNotNull(result.Value);

            // Serialize and Deserialize in two steps
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value);
            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

            Assert.IsNotNull(value);
            Assert.AreEqual(expectedData.Result, (string)value.Result);
        }




        //-------------------------    SaveImageToFolder   -------------------------//

        //23. Testing if imageFiles are null. 
        [Test]
        public async Task SaveImageToFolder_ImageFilesIsNull_ReturnsBadRequest()
        {
            // Arrange
            List<IFormFile> imageFiles = null;
            var descriptions = new List<string> { "Description1" };
            var evaluations = new List<string> { "Evaluation1" };

            // Act
            var result = await _controller.SaveImageToFolder(imageFiles, descriptions, evaluations);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //24. Testing if description are null. 

        [Test]
        public async Task SaveImageToFolder_DescriptionsIsNull_ReturnsBadRequest()
        {
            // Arrange
            var imageFiles = new List<IFormFile> { new FormFile(null, 0, 0, "imageFile", "testImage.jpg") };
            List<string> descriptions = null;
            var evaluations = new List<string> { "Evaluation1" };

            // Act
            var result = await _controller.SaveImageToFolder(imageFiles, descriptions, evaluations);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //25. Testing if evaluation are null. 

        [Test]
        public async Task SaveImageToFolder_EvaluationsIsNull_ReturnsBadRequest()
        {
            // Arrange
            var imageFiles = new List<IFormFile> { new FormFile(null, 0, 0, "imageFile", "testImage.jpg") };
            var descriptions = new List<string> { "Description1" };
            List<string> evaluations = null;

            // Act
            var result = await _controller.SaveImageToFolder(imageFiles, descriptions, evaluations);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //26. Testing if there is a mismatch of length on the different arrays. it should be the same. 
        [Test]
        public async Task SaveImageToFolder_NotSameOfEach_ReturnsBadRequest()
        {
            // Arrange
            var imageFiles = new List<IFormFile> { new FormFile(null, 0, 0, "imageFile", "testImage.jpg") };
            var descriptions = new List<string> { "Description1", "Description2" };
            var evaluations = new List<string> { "Evaluation1" };

            // Act
            var result = await _controller.SaveImageToFolder(imageFiles, descriptions, evaluations);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Invalid request.", badRequestResult.Value);
        }

        //27. Testing error from service.
        [Test]
        public async Task SaveImageToFolder_ImageServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var imageFiles = new List<IFormFile> { new FormFile(null, 0, 0, "imageFile", "testImage.jpg") };
            var descriptions = new List<string> { "Description1" };
            var evaluations = new List<string> { "Evaluation1" };

            _mockImageService
                .Setup(service => service.SaveImageToFolderAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.SaveImageToFolder(imageFiles, descriptions, evaluations);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("An error occurred while processing the image: Test exception", objectResult.Value);
        }






        //-------------------------    DownloadImageWithMetadata   -------------------------//


        //28. Wuen imageFile is null. 
        [Test]
        public async Task DownloadImageWithMetadata_ImageFileIsNull_ReturnsBadRequest()
        {
            // Arrange
            IFormFile imageFile = null; // Null
            string description = "description";
            string evaluation = "evaluation";

            // Act
            var result = await _controller.DownloadImageWithMetadata(imageFile, description, evaluation);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("No image file provided.", badRequestResult.Value);
        }

        //29. When imageFile is empty. 
        [Test]
        public async Task DownloadImageWithMetadata_ImageFileIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var mockStream = new MemoryStream();
            var imageFileMock = new Mock<IFormFile>();
            imageFileMock.Setup(_ => _.Length).Returns(0);// Empty
            var imageFile = imageFileMock.Object;
            string description = "description";
            string evaluation = "evaluation";

            // Act
            var result = await _controller.DownloadImageWithMetadata(imageFile, description, evaluation);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("No image file provided.", badRequestResult.Value);
        }

        //30. When description is null. 
        [Test]
        public async Task DownloadImageWithMetadata_DescriptionIsNull_ReturnsBadRequest()
        {
            // Arrange
            var mockStream = new MemoryStream();
            var imageFileMock = new Mock<IFormFile>();
            imageFileMock.Setup(_ => _.Length).Returns(1);
            var imageFile = imageFileMock.Object;
            string description = null; //Null
            string evaluation = "evaluation";

            // Act
            var result = await _controller.DownloadImageWithMetadata(imageFile, description, evaluation);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("No image file provided.", badRequestResult.Value);
        }

        //31. When description is empty. 
        [Test]
        public async Task DownloadImageWithMetadata_DescriptionIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var mockStream = new MemoryStream();
            var imageFileMock = new Mock<IFormFile>();
            imageFileMock.Setup(_ => _.Length).Returns(1);
            var imageFile = imageFileMock.Object;
            string description = ""; //Empty

            string evaluation = "evaluation";

            // Act
            var result = await _controller.DownloadImageWithMetadata(imageFile, description, evaluation);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("No image file provided.", badRequestResult.Value);
        }

        //32. When evaluation is null. 
        [Test]
        public async Task DownloadImageWithMetadata_EvaluationIsNull_ReturnsBadRequest()
        {
            // Arrange
            var mockStream = new MemoryStream();
            var imageFileMock = new Mock<IFormFile>();
            imageFileMock.Setup(_ => _.Length).Returns(1);
            var imageFile = imageFileMock.Object;
            string description = "description";
            string evaluation = null; //Null 

            // Act
            var result = await _controller.DownloadImageWithMetadata(imageFile, description, evaluation);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("No image file provided.", badRequestResult.Value);
        }

        //33. When evaluation is empty. 
        [Test]
        public async Task DownloadImageWithMetadata_EvaluationIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var mockStream = new MemoryStream();
            var imageFileMock = new Mock<IFormFile>();
            imageFileMock.Setup(_ => _.Length).Returns(1);
            var imageFile = imageFileMock.Object;
            string description = "description";
            string evaluation = ""; //Empty

            // Act
            var result = await _controller.DownloadImageWithMetadata(imageFile, description, evaluation);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("No image file provided.", badRequestResult.Value);
        }

        //32. When failure from service 
        [Test]
        public async Task DownloadImageWithMetadata_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockStream = new MemoryStream();
            var imageFileMock = new Mock<IFormFile>();
            imageFileMock.Setup(_ => _.Length).Returns(1);
            var imageFile = imageFileMock.Object;
            string description = "description";
            string evaluation = "evaluation";

            _mockImageService
                .Setup(service => service.DownloadImageWithMetadataAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DownloadImageWithMetadata(imageFile, description, evaluation);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Error processing your download.", objectResult.Value);
        }



    }

}
