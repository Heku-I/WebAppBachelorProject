using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAppBachelorProject.DAL.Repositories;
using WebAppBachelorProject.Data;
using WebAppBachelorProject.Models;
using Image = WebAppBachelorProject.Models.Image;

namespace WebAppBachelorProject.Controllers
{
    public class GalleryController : Controller


    {

        private readonly ApplicationDbContext _context;
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<GalleryController> _logger;

        public GalleryController(ApplicationDbContext context, IImageRepository imageRepository,
            ILogger<GalleryController> logger)
        {
            _context = context;
            _imageRepository = imageRepository;
            _logger = logger;
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





        public IActionResult DownloadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {

                return BadRequest("Image path is not specified.");
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





    }
}
