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

        Task<bool> UpdateImagePath(string imageId, string path);

        Task<bool> Delete(string imageId);

    }
}
