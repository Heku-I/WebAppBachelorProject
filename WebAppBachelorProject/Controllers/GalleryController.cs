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
        private string? searchString;
        private string? sortOrder;

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

            //filter by description
            if (!string.IsNullOrEmpty(searchString))
            {
                images = images.Where(image => image.Description.Contains(searchString)).ToList();

            }

            _logger.LogInformation("GalleryController: Index has been called");

            //Filter date by descending and ascending 

            ViewData["DateSortParam"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            switch (sortOrder)
            {
                case "date_desc":
                    images = images.OrderByDescending(image => image.DateCreated).ToList();
                    break;
                default:
                    images = images.OrderBy(image => image.DateCreated).ToList();
                    break;
            }
            //Needs Error handling
            _logger.LogInformation("GalleryController: Index has been called");

            return View(images);
        }




    }
}


