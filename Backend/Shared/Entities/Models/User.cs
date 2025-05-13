using Backend.Shared.Entities.Enums;

namespace Backend.Shared.Entities.Models
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public Role Role { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationCodeExpiration { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsPhoneNumberVerified { get; set; }
        public string Password { get; set; }
    }
}