namespace OnlineShoppingCenter.Repositories.Helpers
{
    public static class FileValidator
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] VideoExtensions = { ".mp4", ".webm" };

        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxVideoSize = 50 * 1024 * 1024;
        private const int MaxFilesPerRequest = 10;

        public static (bool IsValid, string Error) Validate(IEnumerable<IFormFile> files)
        {
            var fileList = files.ToList();

            if (fileList.Count > MaxFilesPerRequest)
                return (false, $"Maximum {MaxFilesPerRequest} files allowed per upload.");

            foreach (var file in fileList)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var isImage = ImageExtensions.Contains(ext);
                var isVideo = VideoExtensions.Contains(ext);

                if (!isImage && !isVideo)
                    return (false, $"File type '{ext}' is not allowed. Allowed: {string.Join(", ", ImageExtensions.Concat(VideoExtensions))}");

                if (isImage && file.Length > MaxImageSize)
                    return (false, $"Image '{file.FileName}' exceeds the 5 MB limit.");

                if (isVideo && file.Length > MaxVideoSize)
                    return (false, $"Video '{file.FileName}' exceeds the 50 MB limit.");
            }

            return (true, string.Empty);
        }

        public static string GetFileType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return VideoExtensions.Contains(ext) ? "video" : "image";
        }
    }
}
