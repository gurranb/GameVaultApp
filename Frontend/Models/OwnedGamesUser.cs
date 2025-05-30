using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Models.Steam;

namespace GameVaultApp.Models
{
    public class OwnedGamesUser
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? GameName { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime DateAdded { get; set; }
        public GameVaultAppUser? User { get; set; }
        public List<OwnedGamesUser> Games { get; set; } = new();
    }
}
