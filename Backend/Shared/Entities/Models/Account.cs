namespace Backend.Shared.Entities.Models
{
    public class Account : BaseEntity
    {
        public int UserId { get; set; }
        public int WebsiteId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}