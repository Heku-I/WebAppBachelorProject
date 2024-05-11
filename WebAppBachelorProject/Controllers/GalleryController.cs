using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAppBachelorProject.DAL;
using WebAppBachelorProject.Data;

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


        /*
        //https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-8.0

        [Authorize]
        public async Task<IActionResult> Index(string sortOrder)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"The view has been requested by user ID {userId}");

            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            var userImages = await _imageRepository.GetByUser(userId);

            var imageFiles = from i in userImages
                           select i;

            switch (sortOrder)
            {
                case "Date":
                    imageFiles = imageFiles.OrderBy(i => i.DateCreated);
                    break;
                case "date_desc":
                    imageFiles = imageFiles.OrderByDescending(i => i.DateCreated);
                    break;
                default:
                    imageFiles = imageFiles.OrderByDescending(i => i.DateCreated);
                    break;
            }
            return View(await imageFiles.AsNoTracking().ToListAsync());
        }

        */





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