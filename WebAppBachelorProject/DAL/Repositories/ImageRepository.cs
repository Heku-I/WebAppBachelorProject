using Microsoft.EntityFrameworkCore;
using WebAppBachelorProject.DAL.Context;
using ImageModel = WebAppBachelorProject.Models.Image;

namespace WebAppBachelorProject.DAL.Repositories
{

    public class ImageRepository : IImageRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageRepository> _logger;


        public ImageRepository(ApplicationDbContext context, ILogger<ImageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }



        //Create image record
        public async Task<bool> Create(ImageModel image)
        {
            try
            {
                _context.Images.Add(image);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("[ImageRepository] Saving image failed for {@image}, Error Message: {ex}", image, ex.Message);
                return false;
            }
        }


        //Get all images
        public async Task<IEnumerable<ImageModel>> GetAll()
        {
            return await _context.Images.ToListAsync();

        }




        //Get all the images by the user when logged in 
        public async Task<IEnumerable<ImageModel?>> GetByUser(string userId)
        {

            var images = await _context.Images.Where(e => e.UserId == userId).ToListAsync();

            if (images == null)
            {
                return null;

            }
            return images;

        }


        //Delete an image by its id. 
        public async Task<bool> Delete(string id)
        {

            var image = await _context.Images.FindAsync(id);

            if (image == null)
            {
                return false;
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
            return true;

        }



        //Get an image by it's ID. 
        public async Task<ImageModel> GetByIdAsync(string imageId)
        {
            _logger.LogInformation($"ImageRepository: Fetching image by id {imageId}");
            return await _context.Images.FindAsync(imageId);
        }


        //Update the image
        public async Task UpdateImageAsync(ImageModel image)
        {
            _logger.LogInformation($"ImageRepository: Updating image {image.ImageId}");
            _context.Images.Update(image);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"ImageRepository: Image {image.ImageId} updated in database");
        }



        //Get by user Queryable
        public IQueryable<ImageModel> GetByUserQueryable(string userId)
        {
            return _context.Images.Where(img => img.UserId == userId);
        }


        //Delete an image ASYNC
        public async Task DeleteImageAsync(ImageModel image)
        {
            _logger.LogInformation($"ImageRepository: Deleting image {image.ImageId}");
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"ImageRepository: Image {image.ImageId} deleted from database");
        }


    }
}