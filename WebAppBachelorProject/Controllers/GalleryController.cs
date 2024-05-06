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
        /* Commented our for now.. may be buggy and needs error handling
        [HttpPost]
        public IActionResult searchImages(DateOnly? fromDate, DateOnly? toDate, string description)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _context.Images.Where(i => i.UserId == userId);

            if (!string.IsNullOrEmpty(description))
                query = query.Where(i => i.Description.Contains(description));
            if (fromDate.HasValue)
                query = query.Where(i => i.DateCreated >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(i => i.DateCreated <= toDate.Value);
            var images = query.ToList();
            return View(images);


        }
        */




    }
}