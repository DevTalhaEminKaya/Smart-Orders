namespace Backend.Shared.Entities.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}