using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebAppBachelorProject.Data;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.DAL
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
        public async Task<bool> Create(Image image)
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
    public async Task <IEnumerable<Image>> GetAll()
        {
            return await _context.Images.ToListAsync(); 

        }


        //Get all the images by the user when logged in 
        public async Task<IEnumerable<Image?>> GetByUser(string hostId)
        {

            var images = await _context.Images.Where(e => e.UserId == hostId).ToListAsync();

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
        public async Task<Image?> GetById(int id)
        {

            var image = await _context.Images.FindAsync(id);

            if (image == null)
            {
                return null;

            }
            return image;

        }


        //Update imagepath
        public async Task<bool> UpdateImagesPath(int imageId, string imagePath)
        {
            var images = await _context.Images.FindAsync(imageId);
            if (images == null) return false;

            images.ImagePath = imagePath;


            await _context.SaveChangesAsync();
            return true;
        }

        Task<IEnumerable<Image>> IImageRepository.GetById(string imageId)
        {
            throw new NotImplementedException();
        }

        Task<bool> IImageRepository.UpdateImagePath(string imageId, string path)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Image>> GetByUser()
        {
            throw new NotImplementedException();
        }
    }
}
