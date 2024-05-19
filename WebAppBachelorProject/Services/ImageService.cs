using WebAppBachelorProject.DAL.Repositories;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Microsoft.AspNetCore.Mvc;
using WebAppBachelorProject.Models;


namespace WebAppBachelorProject.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IImageRepository imageRepository, ILogger<ImageService> logger)
        {
            _imageRepository = imageRepository;
            _logger = logger;
        }

        public async Task SaveImageToFolderAsync(IFormFile imageFile, string description, string evaluation, int index)
        {
            _logger.LogInformation($"Processing image {index}");

            string fileName = Path.GetFileName(imageFile.FileName);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            string fullPath = Path.Combine(folderPath, fileName);

            try
            {
                using (var image = AddMetadataToImage(imageFile, description, evaluation))
                {
                    await using (var outputFileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        image.Save(outputFileStream, ImageSharpImage.DetectFormat(imageFile.OpenReadStream()));
                    }
                }

                CheckImageMetadata(fullPath);

                _logger.LogInformation($"Image has been saved successfully. Sending to ImageToDB(desc, path, eval)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing image {fileName}: {ex.Message}");
                throw;
            }
        }


        public async Task<bool> SaveImageToDBAsync(Models.Image image)
        {
            return await _imageRepository.Create(image);
        }




        public ImageSharpImage AddMetadataToImage(IFormFile imageFile, string description, string evaluation)
        {
            _logger.LogInformation("ImageService: AddMetadataToImage has been called.");
            _logger.LogInformation($"Attempting to add the following metadata: Description: {description}, Evaluation: {evaluation}");

            var imageStream = imageFile.OpenReadStream();
            var format = ImageSharpImage.DetectFormat(imageStream);
            _logger.LogInformation($"Detected format: {format.Name}");
            imageStream.Position = 0;  // Reset stream position after format detection

            var image = ImageSharpImage.Load(imageStream);

            var metadata = image.Metadata.ExifProfile ?? new ExifProfile();
            metadata.SetValue(ExifTag.ImageDescription, description);
            metadata.SetValue(ExifTag.UserComment, evaluation);
            image.Metadata.ExifProfile = metadata;

            _logger.LogInformation("Metadata is added, returning image.");

            return image;
        }





        public void CheckImageMetadata(string imagePath)
        {
            using (var image = ImageSharpImage.Load(imagePath))
            {
                var metadata = image.Metadata.ExifProfile;
                if (metadata != null)
                {
                    if (metadata.TryGetValue(ExifTag.ImageDescription, out var imageDescription))
                    {
                        _logger.LogInformation("Image Description from metadata: " + imageDescription.GetValue());
                    }
                    else
                    {
                        _logger.LogError("No Image Description set in EXIF data.");
                    }
                }
                else
                {
                    _logger.LogError($"No EXIF metadata found.");
                }
            }
        }


        //NEW: 

        public async Task<FileContentResult> DownloadImageWithMetadataAsync(IFormFile imageFile, string description, string evaluation)
        {
            _logger.LogInformation("ImageProcessingService: DownloadImageWithMetadataAsync is reached.");

            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("No image file provided.");
            }

            try
            {
                using var imageStream = imageFile.OpenReadStream();
                var format = ImageSharpImage.DetectFormat(imageStream);  // Detect the format using the stream
                if (format == null)
                {
                    _logger.LogError("Unsupported image format.");
                    throw new ArgumentException("Unsupported image format.");
                }

                imageStream.Position = 0;  // Reset the stream position after detecting the format

                // Use the existing function to add metadata to the image
                using var image = AddMetadataToImage(imageFile, description, evaluation);
                var memoryStream = new MemoryStream();

                image.Save(memoryStream, format);  
                memoryStream.Position = 0; 

                string mimeType = format.DefaultMimeType;  
                return new FileContentResult(memoryStream.ToArray(), mimeType)
                {
                    FileDownloadName = Path.GetFileName(imageFile.FileName)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while processing image download: {ex.Message}");
                throw;
            }
        }



        public async Task UpdateImgDescAsync(string imageId, string description)
        {
            _logger.LogInformation("ImageService: UpdateImgDescAsync is reached.");

            var image = await _imageRepository.GetByIdAsync(imageId);

            if (image != null)
            {
                _logger.LogInformation("ImageService: Updating description.");
                image.Description = description;
                await _imageRepository.UpdateImageAsync(image);
                _logger.LogInformation("ImageService: Image updated successfully.");
            }
            else
            {
                _logger.LogWarning("ImageService: Image not found.");
            }
        }








    }

}




