using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebAppBachelorProject.Controllers;
using WebAppBachelorProject.DAL.Context;
using WebAppBachelorProject.DAL.Repositories;
using ImageModel = WebAppBachelorProject.Models.Image;
using WebAppBachelorProject.Services;
using static WebAppBachelorProject.Services.ImageProcessingService;

namespace WebApp.Tests
{

    [TestFixture]
    public class GalleryControllerTests
    {
        private Mock<ILogger<GalleryController>> _mockLogger;
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<IImageRepository> _mockImageRepository;
        private Mock<IImageService> _mockImageService;
        private GalleryController _controller;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<GalleryController>>();
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockImageRepository = new Mock<IImageRepository>();
            _mockImageService = new Mock<IImageService>();

            _controller = new GalleryController(
                _mockContext.Object,
                _mockImageRepository.Object,
                _mockLogger.Object,
                _mockImageService.Object
            );

            // Mocking the user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "testUserId")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }


        //------------------------- Function: Index -------------------------//

        //1: testing if user is null (not logged in).
        [Test]
        public async Task Index_UserIdIsNull_ReturnsForbid()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No user claims

            // Act
            var result = await _controller.Index(null, null, null, null);

            // Assert
            Assert.IsInstanceOf<ForbidResult>(result);
        }





        //------------------------- Function: DownloadImage -------------------------//

        //2: Testing if imagePath is null
        [Test]
        public void DownloadImage_imagePathIsNull_ReturnsBR()
        {

            //Arrange            
            var result = _controller.DownloadImage(null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }



        //3: Testing if imagePath is empty
        [Test]
        public void DownloadImage_ImagePathIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            string imagePath = string.Empty;
            // Act
            var result = _controller.DownloadImage(imagePath);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Image path is not specified.", badRequestResult.Value);
        }








    }
}
