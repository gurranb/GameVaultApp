namespace GameVaultApi.Models.Steam
{
    public class Profile
    {
        public string? SteamId { get; set; }
        public string? Avatar { get; set; }
        public string? AvatarFull { get; set; }
        public string? Personaname { get; set; }
        public string? Profileurl { get; set; }
    }

    public class SteamProfileResponse
    {
        public SteamProfileResponseData? Response { get; set; }
    }

    public class SteamProfileResponseData
    {
        public List<Models.Steam.Profile>? Players { get; set; }
    }
}
