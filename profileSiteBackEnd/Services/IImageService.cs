namespace profileSiteBackEnd.Services
{
    public interface IImageService
    {
        Task<string> SaveProjectImageAsync(IFormFile imageFile, string projectSlug);
        Task DeleteProjectImageAsync(string? imageUrl);
        bool ValidateImage(IFormFile file);
    }
}
