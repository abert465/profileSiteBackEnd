using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace profileSiteBackEnd.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;
        private const int MaxWidth = 1200;
        private const int MaxHeight = 800;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public bool ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return false;

            return true;
        }

        public async Task<string> SaveProjectImageAsync(IFormFile imageFile, string projectSlug)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "projects");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"{projectSlug}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var image = await Image.LoadAsync(imageFile.OpenReadStream()))
                {
                    if (image.Width > MaxWidth || image.Height > MaxHeight)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(MaxWidth, MaxHeight),
                            Mode = ResizeMode.Max
                        }));
                    }

                    if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                    {
                        await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });
                    }
                    else if (fileExtension == ".png")
                    {
                        await image.SaveAsPngAsync(filePath, new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression });
                    }
                    else if (fileExtension == ".webp")
                    {
                        await image.SaveAsWebpAsync(filePath, new WebpEncoder { Quality = 85 });
                    }
                    else
                    {
                        await image.SaveAsync(filePath);
                    }
                }

                return $"/uploads/projects/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving project image for slug: {Slug}", projectSlug);
                throw;
            }
        }

        public async Task DeleteProjectImageAsync(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            try
            {
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "projects", fileName);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation("Deleted image: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project image: {ImageUrl}", imageUrl);
            }
        }
    }
}
