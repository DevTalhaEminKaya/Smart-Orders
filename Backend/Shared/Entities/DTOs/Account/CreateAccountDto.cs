namespace Backend.Shared.Entities.DTOs.Account
{
    public class CreateAccountDto
    {
        public int WebsiteId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
