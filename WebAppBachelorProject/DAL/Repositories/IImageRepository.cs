﻿using WebAppBachelorProject.Models;
using ImageModel = WebAppBachelorProject.Models.Image;

namespace WebAppBachelorProject.DAL.Repositories
{
    public interface IImageRepository
    {

        Task<bool> Create(ImageModel image);

        Task<IEnumerable<ImageModel>> GetAll();

        Task<IEnumerable<ImageModel>> GetByUser(string userId);

        Task<bool> Delete(string imageId);

        Task<ImageModel> GetByIdAsync(string imageId);

        Task UpdateImageAsync(ImageModel image);

        public IQueryable<ImageModel> GetByUserQueryable(string userId);

        Task DeleteImageAsync(ImageModel image);

    }
}
