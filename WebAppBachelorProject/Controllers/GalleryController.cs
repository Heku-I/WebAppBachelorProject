using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAppBachelorProject.DAL.Context;
using WebAppBachelorProject.DAL.Repositories;
using WebAppBachelorProject.Models;
using WebAppBachelorProject.Services;
using Image = WebAppBachelorProject.Models.Image;

namespace WebAppBachelorProject.Controllers
{
    [Authorize]
    public class GalleryController : Controller


    {

        private readonly ApplicationDbContext _context;
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<GalleryController> _logger;
        private readonly IImageService _imageService;


        public GalleryController(
            ApplicationDbContext context, 
            IImageRepository imageRepository,
            ILogger<GalleryController> logger, 
            IImageService imageService)
        {
            _context = context;
            _imageRepository = imageRepository;
            _logger = logger;
            _imageService = imageService;

        }


        //https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-8.0
        [Authorize]
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber
        )

        {
            // Retrieve the user ID from the current user claims
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"The gallery has been requested by user ID {userId}");

            // Check if the user ID is null and handle it accordingly
            if (userId == null)
            {
                _logger.LogError("No User");
                return Forbid("No user");
            }

            // Handle the search string and page number
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var userImages = _imageRepository.GetByUserQueryable(userId);

            var images = from s in userImages
                         select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                images = images.Where(s => s.Description.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Date":
                    images = images.OrderBy(i => i.DateCreated);
                    break;
                case "date_desc":
                    images = images.OrderByDescending(i => i.DateCreated);
                    break;
                default:
                    images = images.OrderByDescending(i => i.DateCreated);
                    break;
            }

            int pageSize = 3;

            return View(await PaginatedList<Image>.CreateAsync(images.AsNoTracking(), pageNumber ?? 1, pageSize));
        }




        [Authorize]
        public IActionResult DownloadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {

                return BadRequest("Image path is not specified.");
            }

            if (imagePath.StartsWith("/"))
            {
                imagePath = imagePath.TrimStart('/');
            }

            _logger.LogInformation($"Imagepath: {imagePath}"); 

            var path = Path.Combine("wwwroot", imagePath);

            if (!System.IO.File.Exists(path))
            {
                _logger.LogError($"Image is not found with this path: {path}");
                return NotFound("Image not found.");
            }

            var fileName = Path.GetFileName(path);
            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateImageDescription(UpdateImageDescriptionRequest request)
        {
            _logger.LogInformation("GalleryController: UpdateImageDescription is reached.");

            if (request == null)
            {
                _logger.LogWarning("Empty request received.");
                return NotFound("Empty request.");
            }

            _logger.LogInformation($"Description: {request.Description}");
            _logger.LogInformation($"ImageId: {request.ImageId}");

            var image = await _imageRepository.GetByIdAsync(request.ImageId);

            if (image == null)
            {
                _logger.LogWarning($"GalleryController: Image not found: {request.ImageId}");
                return NotFound("GalleryController: Image not found.");
            }

            try
            {
                await _imageService.UpdateImgDescAsync(request.ImageId, request.Description);
                _logger.LogInformation("GalleryController: Returning to index");
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while updating the image description.");
                return StatusCode(500, "Internal server error");
            }
        }



        //THIS NEEDS TO BE UNDER MODELS!!!
        public class UpdateImageDescriptionRequest
        {
            public string ImageId { get; set; }
            public string Description { get; set; }
        }









    }
}
