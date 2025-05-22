namespace GameVaultApi.DTO
{
    public class UserDto
    {
        public string Id { get; set; }             // Corresponds to UserId
        public string FirstName { get; set; }      // Optional, if you want to expose this
        public string LastName { get; set; }       // Optional
        public string SteamId { get; set; }        // Optional
    }
}
