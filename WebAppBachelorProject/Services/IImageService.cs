namespace WebAppBachelorProject.Services
{
    using Microsoft.AspNetCore.Mvc;
    using ImageSharpImage = SixLabors.ImageSharp.Image;

    public interface IImageService
    {
        Task SaveImageToFolderAsync(IFormFile imageFile, string description, string evaluation, int index);
        ImageSharpImage AddMetadataToImage(IFormFile imageFile, string description, string evaluation);
        Task<bool> SaveImageToDBAsync(Models.Image image);
        void CheckImageMetadata(string imagePath);
        Task<FileContentResult> DownloadImageWithMetadataAsync(IFormFile imageFile, string description, string evaluation);

        Task UpdateImgDescAsync(string imageId, string description);

    }
}
