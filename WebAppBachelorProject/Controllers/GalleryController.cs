using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using WebAppBachelorProject.DAL;
using WebAppBachelorProject.Data;

namespace WebAppBachelorProject.Controllers
{
    public class GalleryController : Controller


    {

        private readonly ApplicationDbContext _context;
        private readonly IImageRepository _imageRepository;
        private readonly Logger<ImageRepository> _logger;

        public GalleryController(ApplicationDbContext context, IImageRepository imageRepository, Logger<ImageRepository> logger)
        {
            _context = context;
            _imageRepository = imageRepository;
            _logger = logger;
        }



        // GET: Gallery
        [Authorize]
        public IActionResult Index()
        {


            return View();
        }




        public async Task<IActionResult> GetByUser()
        {
            _logger.LogInformation("GallaryController: Index has been called.");

            var uploadedImages = await _imageRepository.GetByUser();

            return uploadedImages == null ? NotFound() : Ok(uploadedImages);
        }




    }
}


