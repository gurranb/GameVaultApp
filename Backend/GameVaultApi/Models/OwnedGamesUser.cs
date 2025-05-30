namespace GameVaultApi.Models
{
    public class OwnedGamesUser
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? GameName { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
