using WebAppBachelorProject.Models;
using ImageModel = WebAppBachelorProject.Models.Image;

namespace WebAppBachelorProject.DAL.Repositories
{
    public interface IImageRepository
    {

        Task<bool> Create(ImageModel image);

        Task<IEnumerable<ImageModel>> GetAll();

        Task<IEnumerable<ImageModel>> GetById(string imageId);

        Task<IEnumerable<ImageModel>> GetByUser(string userId);

        Task<bool> Delete(string imageId);

        public IQueryable<ImageModel> GetByUserQueryable(string userId);

        Task<ImageModel> GetByIdAsync(string imageId);

        Task UpdateImageAsync(ImageModel image);

        Task DeleteImageAsync(ImageModel image);

    }
}
