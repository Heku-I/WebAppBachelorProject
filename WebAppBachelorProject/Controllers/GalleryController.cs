using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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




        //[Authorize]
        public IActionResult Index()
        {
            return View();
        }



        /*
        public async Task<IActionResult> GetByUser()
        {
            _logger.LogInformation("GallaryController: Index has been called.");

            var uploadedImages = await _imageRepository.GetByUser();

            return uploadedImages == null ? NotFound() : Ok(uploadedImages);
        }
        */



    }
}


