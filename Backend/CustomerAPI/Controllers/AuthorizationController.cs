using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Backend.Shared.Entities.Enums;
using Backend.Shared.Entities.Models;
using Backend.Shared.Entities.DTOs.Auth;
using Backend.Shared.Entities;
using Backend.Shared.DataAccess;

namespace ClientApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase 
    {
        private readonly SmartOrdersDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private int _userId;

        public AuthorizationController(SmartOrdersDbContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        // ----------------------------------------------------------------------------------------------------

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new Exception("Bu email zaten kayıtlı.");

            var random = new Random();

            var user = new User
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                Role = Role.User,
                IsEmailVerified = false,
                EmailVerificationCode = random.Next(100000, 999999).ToString(),
                EmailVerificationCodeExpiration = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("verification@smartorders.com"));
            email.To.Add(MailboxAddress.Parse(dto.Email));
            email.Subject = "Doğrulama Kodunuz";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = $"Kodunuz: {user.EmailVerificationCode}" };
    
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("192.168.20.161", 8003, SecureSocketOptions.None);
            await smtp.AuthenticateAsync("talhaeminkaya@gmail.com", "");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return Ok(new { message = "Kayıt başarılı" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCryptNet.Verify(dto.Password, user.Password))
                throw new Exception("Email veya parola hatalı.");

            // JWT token oluştur
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)); // _jwtSettings'i kullan
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_jwtSettings.DurationInMinutes.ToString())),
                signingCredentials: creds
            );

            AuthResponseDto authResponse = new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };

            return Ok(authResponse);
        }

        [Authorize]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] string code)
        {
            // Token'dan gelen userId'yi al
            _userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == _userId);

            if (user == null)
                return NotFound("User not found.");

            if (user.IsEmailVerified)
                return BadRequest("Email is already verified.");

            if (user.EmailVerificationCode != code)
                return BadRequest("The code is incorrect.");

            if (user.EmailVerificationCodeExpiration < DateTime.UtcNow)
                return BadRequest("Code has expired.");

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationCodeExpiration = null;

            await _context.SaveChangesAsync();

            return Ok("Email verified.");
        }
    }
}