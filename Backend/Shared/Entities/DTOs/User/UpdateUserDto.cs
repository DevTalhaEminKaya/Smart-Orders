using Backend.Shared.Entities.Enums;

namespace Backend.Shared.Entities.DTOs.User
{
    public class UpdateUserDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public Role Role { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}