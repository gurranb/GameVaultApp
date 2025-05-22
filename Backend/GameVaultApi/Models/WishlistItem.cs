
namespace GameVaultApi.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AppId { get; set; }
        public string? Name { get; set; }
        public string? LogoUrl { get; set; }
        public string? IconUrl { get; set; }
        public DateTime DateAdded { get; set; }

    }
}
