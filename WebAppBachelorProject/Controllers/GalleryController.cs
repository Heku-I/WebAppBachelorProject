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

        public GalleryController(ApplicationDbContext context, IImageRepository imageRepository, ILogger<GalleryController> logger)
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

            var userImages = await _imageRepository.GetByUser(userId);

            var images = from s in _context.Images
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                images = images.Where(s => s.Description.Contains(searchString));
            }

            //Sorting not working yet!
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



    }
}