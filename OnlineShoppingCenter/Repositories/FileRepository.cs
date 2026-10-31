using OnlineShoppingCenter.Data;
using OnlineShoppingCenter.Models;
using OnlineShoppingCenter.Repositories.Helpers;
using OnlineShoppingCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace OnlineShoppingCenter.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FileRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<FileEntity>> UploadFilesAsync(IEnumerable<IFormFile> files, string ownerType, int ownerId)
        {
            var uploaded = new List<FileEntity>();
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", ownerType.ToLower(), ownerId.ToString());
            Directory.CreateDirectory(uploadDir);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var newFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, newFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var entity = new FileEntity
                {
                    Type = FileValidator.GetFileType(file.FileName),
                    OwnerType = ownerType,
                    OwnerId = ownerId,
                    Path = $"/uploads/{ownerType.ToLower()}/{ownerId}/{newFileName}",
                    OriginalName = file.FileName,
                    Size = file.Length
                };
                _context.Files.Add(entity);
                uploaded.Add(entity);
            }

            return uploaded;
        }

        public async Task DeleteFilesByOwnerAsync(string ownerType, int ownerId)
        {
            var files = await _context.Files
                .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId)
                .ToListAsync();

            foreach (var file in files)
            {
                var fullPath = Path.Combine(_env.WebRootPath, file.Path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }

            _context.Files.RemoveRange(files);
        }

        public async Task ReplaceFilesAsync(IEnumerable<IFormFile> files, string ownerType, int ownerId)
        {
            await DeleteFilesByOwnerAsync(ownerType, ownerId);
            await UploadFilesAsync(files, ownerType, ownerId);
        }

        public async Task<List<FileEntity>> GetFilesByOwnerAsync(string ownerType, int ownerId)
        {
            return await _context.Files
                .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId)
                .ToListAsync();
        }
    }
}
