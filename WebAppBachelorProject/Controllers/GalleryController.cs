using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAppBachelorProject.DAL;
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

        public GalleryController(ApplicationDbContext context, IImageRepository imageRepository, ILogger<GalleryController> logger)
        {
            _context = context;
            _imageRepository = imageRepository;
            _logger = logger;
        }



        /*
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"The view has been requested by user ID {userId}");


            var images = await _imageRepository.GetByUser(userId);

            _logger.LogInformation("GalleryController: Index has been called.");

            //ViewData["User"] = User;


            return View(images);
        }
        */





        //https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-8.0

        [Authorize]
        public async Task<IActionResult> Index(
             string sortOrder,
             string currentFilter,
             string searchString,
             int? pageNumber
            )
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"The gallery has been requested by user ID {userId}");


            if(userId == null)
            {
                _logger.LogError("No User"); 
                Forbid("No user");
            }


            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }


            ViewData["CurrentFilter"] = searchString;

            // Await the task to get the collection
            var userImages = await _imageRepository.GetByUser(userId);

            var images = from s in _context.Images
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









        /* Commented our for now.. may be buggy and needs error handlings
        [HttpPost]
        public IActionResult searchImages(DateOnly? fromDate, DateOnly? toDate, string description)
        {
            try
            {
                Console.WriteLine("searchImages has been calles");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var query = _context.Images.Where(i => i.UserId == userId);

                if (!string.IsNullOrEmpty(description))
                    query = query.Where(i => i.Description.Contains(description));
                if (fromDate.HasValue)
                    query = query.Where(i => i.DateCreated >= fromDate.Value);
                if (toDate.HasValue)
                    query = query.Where(i => i.DateCreated <= toDate.Value);
                var images = query.ToList();

                if (images.Count == 0)
                {
                    TempData["ErrorMessage"] = "No images found matching the search ";

                }
                return View(images);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occurred while processing");
                TempData["ErrorMessage"] = "An error occurred while processing";
                return View();
            }

        }*/

    }
}