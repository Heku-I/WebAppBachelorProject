using WebAppBachelorProject.Models;
using ImageModel = WebAppBachelorProject.Models.Image;

namespace WebAppBachelorProject.DAL
{
    public interface IImageRepository
    {

        Task<bool> Create(ImageModel image);

        Task<IEnumerable<ImageModel>> GetAll();

        Task<IEnumerable<ImageModel>> GetById(string imageId);

        Task<IEnumerable<ImageModel>> GetByUser(); 

        Task<bool> UpdateImagePath (string imageId,  string path);

        Task<bool> Delete(string imageId); 

    }
}
