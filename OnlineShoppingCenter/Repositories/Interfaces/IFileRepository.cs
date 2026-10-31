using OnlineShoppingCenter.Models;

namespace OnlineShoppingCenter.Repositories.Interfaces
{
    public interface IFileRepository
    {
        Task<List<FileEntity>> UploadFilesAsync(IEnumerable<IFormFile> files, string ownerType, int ownerId);
        Task DeleteFilesByOwnerAsync(string ownerType, int ownerId);
        Task ReplaceFilesAsync(IEnumerable<IFormFile> files, string ownerType, int ownerId);
        Task<List<FileEntity>> GetFilesByOwnerAsync(string ownerType, int ownerId);
    }
}
