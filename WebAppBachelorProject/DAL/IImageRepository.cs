using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.DAL
{
    public interface IImageRepository
    {

        Task<bool> Create(Image image);

        Task<IEnumerable<Image>> GetAll();

        Task<IEnumerable<Image>> GetById(string imageId);

        Task<IEnumerable<Image>> GetByUser(); 

        Task<bool> UpdateImagePath (string imageId,  string path);

        Task<bool> Delete(string imageId); 

    }
}
