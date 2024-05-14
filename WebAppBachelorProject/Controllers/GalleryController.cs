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

            // Store the current filter in ViewData for use in the view
            ViewData["CurrentFilter"] = searchString;

            // Fetch images associated with the user
            var userImages = _imageRepository.GetByUserQueryable(userId);

            // Apply filtering based on the search string
            var images = from s in userImages
                         select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                images = images.Where(s => s.Description.Contains(searchString));
            }

            // Apply sorting based on the sortOrder parameter
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

            // Define the page size
            int pageSize = 3;

            // Return the view with paginated list of images
            return View(await PaginatedList<Image>.CreateAsync(images.AsNoTracking(), pageNumber ?? 1, pageSize));
        }



    }
}
