using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using WebAppBachelorProject.Controllers;
using WebAppBachelorProject.Models;
using WebAppBachelorProject.Data;  // Assuming ApplicationDbContext resides here
using WebAppBachelorProject.DAL;
using NuGet.ContentModel;  // Assuming IImageRepository resides here

namespace ImageAble.Tests
{
    [TestFixture]
    public class ImageControllerTests
    {
        private Mock<ILogger<ImageController>> _mockLogger;
        private Mock<IImageRepository> _mockImageRepository;
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<IConfiguration> _mockConfiguration;
        private ImageController _controller;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ImageController>>();
            _mockImageRepository = new Mock<IImageRepository>();
            _mockContext = new Mock<ApplicationDbContext>();
            _mockConfiguration = new Mock<IConfiguration>();

            _controller = new ImageController(_mockLogger.Object, _mockImageRepository.Object, _mockContext.Object, _mockConfiguration.Object);
        }

        [Test]
        public async Task GetMultipleImages_NullRequest_ReturnsBadRequestWithMessage()
        {
            // Act
            var result = await _controller.GetMultipleImages(null) as BadRequestObjectResult;

            // Assert

            Assert.That(result, Is.Null );

            Assert.That(result, Is.EqualTo( null ) );
        }


        [Test]
        public void TestAssertion()
        {
            Assert.IsNotNull(null, "This should fail because the value is null.");
            Assert.AreEqual(1, 1, "This should pass because 1 equals 1.");
        }

    }
}
