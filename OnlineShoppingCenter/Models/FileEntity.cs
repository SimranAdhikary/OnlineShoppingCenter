namespace OnlineShoppingCenter.Models
{
    public class FileEntity : BaseEntity
    {
        public string Type { get; set; } = string.Empty;
        public string OwnerType { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string Path { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
